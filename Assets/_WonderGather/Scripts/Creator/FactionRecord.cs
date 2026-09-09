using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
namespace WonderGather
{
    public sealed class FactionUnitRecord
    {
        public string Id,Name;
        public int Start;
        public bool Gathers,Builds;
    }
    // Version 2 stores independent units and one explicit workshop training link.
    public sealed class FactionRecord
    {
        public const int Version=2;
        public string Id,Name,BaseId,WorkerId,WorkshopId,TrainedId;
        public int Supplies;
        public FactionUnitRecord[] Units;
        public int Workers=>Units.Sum(x=>x.Start);
        private const string LegacyFields="version template id name base worker workshop workers supplies gathers builds trains";
        private const string Fields="version template id name base worker workshop supplies units trained";
        public static FactionRecord Capture(FactionDraft draft,string id)=>new FactionRecord{
            Id=id,Name=draft.Definition.DisplayName,BaseId=draft.Definition.StartingBase.Id,WorkerId=draft.TemplateWorkerId,WorkshopId=draft.Workshop.Id,
            Supplies=draft.Definition.StartingSupplies,TrainedId=draft.Workshop.Produces?.Id,
            Units=draft.Units.Select(x=>new FactionUnitRecord{Id=x.Id,Name=x.DisplayName,Start=draft.StartingCount(x),Gathers=x.GathersSupplies,Builds=x.CanBuild(draft.Workshop)}).ToArray()};
        public void Validate()
        {
            if(!Guid.TryParseExact(Id,"N",out _)) throw new InvalidDataException("The faction file has an invalid ID.");
            if(!FactionDraft.ValidName(Name)) throw new InvalidDataException("Faction names need 1–64 printable characters.");
            if(!FactionDraft.ValidName(BaseId)||!FactionDraft.ValidName(WorkerId)||!FactionDraft.ValidName(WorkshopId)||BaseId==WorkshopId) throw new InvalidDataException("Blueprint template IDs are missing or invalid.");
            if(Supplies<0 || Supplies>120 || Units==null || Units.Length<1 || Units.Length>FactionDraft.MaxUnitBlueprints) throw new InvalidDataException("Faction values exceed this prototype's supported limits.");
            var ids=new HashSet<string>{BaseId,WorkshopId};int total=0;
            foreach(var unit in Units)
            {
                if(unit==null || !FactionDraft.ValidName(unit.Id) || !ids.Add(unit.Id) || !FactionDraft.ValidName(unit.Name)) throw new InvalidDataException("Unit blueprint IDs must be unique and names need 1–64 printable characters.");
                if(unit.Start<0 || unit.Start>FactionDraft.MaxStartingUnits) throw new InvalidDataException("Invalid starting unit count.");
                total+=unit.Start;
            }
            if(total>FactionDraft.MaxStartingUnits) throw new InvalidDataException("Too many starting units for this test map.");
            if(TrainedId!=null && !Units.Any(x=>x.Id==TrainedId)) throw new InvalidDataException("The workshop training link references a missing unit.");
        }
        public string Encode()
        {
            Validate();
            using(var stream=new MemoryStream())
            {
                using(var json=new Utf8JsonWriter(stream,new JsonWriterOptions{Indented=true}))
                {
                    json.WriteStartObject();json.WriteNumber("version",Version);json.WriteString("template","settlement-1");
                    json.WriteString("id",Id);json.WriteString("name",Name);json.WriteString("base",BaseId);json.WriteString("worker",WorkerId);json.WriteString("workshop",WorkshopId);
                    json.WriteNumber("supplies",Supplies);json.WriteString("trained",TrainedId);json.WriteStartArray("units");
                    foreach(var unit in Units)
                    {
                        json.WriteStartObject();json.WriteString("id",unit.Id);json.WriteString("name",unit.Name);json.WriteNumber("start",unit.Start);
                        json.WriteBoolean("gathers",unit.Gathers);json.WriteBoolean("builds",unit.Builds);json.WriteEndObject();
                    }
                    json.WriteEndArray();json.WriteEndObject();
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        private static void CheckFields(JsonElement value,string expected)
        {
            if(value.ValueKind!=JsonValueKind.Object) throw new InvalidDataException("Expected a faction object.");
            var fields=new HashSet<string>(expected.Split(' '));var seen=new HashSet<string>();
            foreach(var field in value.EnumerateObject()) if(!fields.Contains(field.Name)||!seen.Add(field.Name)) throw new InvalidDataException("This faction contains unknown or duplicate fields; it will not be rewritten.");
            if(seen.Count!=fields.Count) throw new InvalidDataException("This faction file is incomplete.");
        }
        public static FactionRecord Decode(string text)
        {
            try
            {
                using(var document=JsonDocument.Parse(text,new JsonDocumentOptions{MaxDepth=8}))
                {
                    var root=document.RootElement;
                    if(root.ValueKind!=JsonValueKind.Object || !root.TryGetProperty("version",out var version)||!version.TryGetInt32(out var number)||(number!=1 && number!=Version))
                        throw new InvalidDataException("This faction uses a missing or unsupported file version. Its file has been left unchanged.");
                    CheckFields(root,number==1?LegacyFields:Fields);
                    if(root.GetProperty("template").GetString()!="settlement-1") throw new InvalidDataException("This faction needs an unsupported blueprint collection.");
                    var record=new FactionRecord{Id=root.GetProperty("id").GetString(),Name=root.GetProperty("name").GetString(),BaseId=root.GetProperty("base").GetString(),WorkerId=root.GetProperty("worker").GetString(),WorkshopId=root.GetProperty("workshop").GetString(),Supplies=root.GetProperty("supplies").GetInt32()};
                    if(number==1)
                    {
                        record.Units=new[]{new FactionUnitRecord{Id=record.WorkerId,Name="Worker",Start=root.GetProperty("workers").GetInt32(),Gathers=root.GetProperty("gathers").GetBoolean(),Builds=root.GetProperty("builds").GetBoolean()}};
                        record.TrainedId=root.GetProperty("trains").GetBoolean()?record.WorkerId:null;
                    }
                    else
                    {
                        record.TrainedId=root.GetProperty("trained").GetString();var list=new List<FactionUnitRecord>();
                        foreach(var unit in root.GetProperty("units").EnumerateArray())
                        {
                            if(list.Count>=FactionDraft.MaxUnitBlueprints) throw new InvalidDataException("Too many unit blueprints.");
                            CheckFields(unit,"id name start gathers builds");
                            list.Add(new FactionUnitRecord{Id=unit.GetProperty("id").GetString(),Name=unit.GetProperty("name").GetString(),Start=unit.GetProperty("start").GetInt32(),Gathers=unit.GetProperty("gathers").GetBoolean(),Builds=unit.GetProperty("builds").GetBoolean()});
                        }
                        record.Units=list.ToArray();
                    }
                    record.Validate();return record;
                }
            }
            catch(Exception error) when(error is JsonException || error is InvalidOperationException || error is FormatException || error is OverflowException)
            {throw new InvalidDataException("This faction file is damaged or contains invalid values.",error);}
        }
        public FactionDraft CreateDraft(CivilizationDefinition template)
        {
            Validate();var draft=new FactionDraft(template);
            try
            {
                if(draft.Definition.StartingBase.Id!=BaseId || draft.TemplateWorkerId!=WorkerId || draft.Workshop.Id!=WorkshopId) throw new InvalidDataException("One or more blueprint IDs are unavailable in this version of the game.");
                draft.RestoreUnits(Units,TrainedId);draft.SetFactionSetup(Name,Supplies);return draft;
            }
            catch{draft.Dispose();throw;}
        }
    }
}
