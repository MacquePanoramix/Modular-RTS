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
        public bool Gathers;
        public string[] BuildIds;
    }
    public sealed class FactionBuildingRecord
    {
        public string Id,Name;
        public string[] Trains;
    }
    public sealed class FactionRecord
    {
        public const int Version=3;
        public string Id,Name,BaseId,WorkerId,WorkshopId;
        public int Supplies;
        public FactionUnitRecord[] Units;
        public FactionBuildingRecord[] Buildings;
        public int Workers=>Units.Sum(x=>x.Start);
        private const string LegacyFields="version template id name base worker workshop workers supplies gathers builds trains";
        private const string SecondFields="version template id name base worker workshop supplies units trained";
        private const string Fields="version template id name base worker workshop supplies units buildings";
        public static FactionRecord Capture(FactionDraft draft,string id)=>new FactionRecord{
            Id=id,Name=draft.Definition.DisplayName,BaseId=draft.Definition.StartingBase.Id,WorkerId=draft.TemplateWorkerId,WorkshopId=draft.TemplateWorkshopId,
            Supplies=draft.Definition.StartingSupplies,
            Units=draft.Units.Select(x=>new FactionUnitRecord{Id=x.Id,Name=x.DisplayName,Start=draft.StartingCount(x),Gathers=x.GathersSupplies,BuildIds=x.Builds.Select(b=>b.Id).ToArray()}).ToArray(),
            Buildings=draft.Buildings.Select(x=>new FactionBuildingRecord{Id=x.Id,Name=x.DisplayName,Trains=x.ProductionOptions.Select(u=>u.Id).ToArray()}).ToArray()};
        private static void Links(string[] links,HashSet<string> roster)
        {
            if(links==null || links.Length>8 || links.Any(x=>x==null || !roster.Contains(x)) || links.Distinct().Count()!=links.Length)
                throw new InvalidDataException("Blueprint links must reference distinct entries in the faction roster.");
        }
        public void Validate()
        {
            if(!Guid.TryParseExact(Id,"N",out _)) throw new InvalidDataException("The faction file has an invalid ID.");
            if(!FactionDraft.ValidName(Name)) throw new InvalidDataException("Faction names need 1–64 printable characters.");
            if(!FactionDraft.ValidName(BaseId)||!FactionDraft.ValidName(WorkerId)||!FactionDraft.ValidName(WorkshopId)||BaseId==WorkshopId) throw new InvalidDataException("Blueprint template IDs are missing or invalid.");
            if(Supplies<0 || Supplies>120 || Units==null || Units.Length<1 || Units.Length>FactionDraft.MaxUnitBlueprints || Buildings==null || Buildings.Length<1 || Buildings.Length>FactionDraft.MaxBuildingBlueprints) throw new InvalidDataException("Faction values exceed this prototype's supported limits.");
            var ids=new HashSet<string>{BaseId};int total=0;
            foreach(var unit in Units)
            {
                if(unit==null || !FactionDraft.ValidName(unit.Id) || !ids.Add(unit.Id) || !FactionDraft.ValidName(unit.Name)) throw new InvalidDataException("Blueprint IDs must be unique and names need 1–64 printable characters.");
                if(unit.Start<0 || unit.Start>FactionDraft.MaxStartingUnits) throw new InvalidDataException("Invalid starting unit count.");total+=unit.Start;
            }
            foreach(var building in Buildings)
                if(building==null || !FactionDraft.ValidName(building.Id) || !ids.Add(building.Id) || !FactionDraft.ValidName(building.Name)) throw new InvalidDataException("Building blueprint IDs must be unique and names printable.");
            if(total>FactionDraft.MaxStartingUnits) throw new InvalidDataException("Too many starting units for this test map.");
            var units=new HashSet<string>(Units.Select(x=>x.Id));var buildings=new HashSet<string>(Buildings.Select(x=>x.Id));
            foreach(var unit in Units) Links(unit.BuildIds,buildings);
            foreach(var building in Buildings) Links(building.Trains,units);
        }
        private static void WriteLinks(Utf8JsonWriter json,string name,string[] links)
        {json.WriteStartArray(name);foreach(string id in links) json.WriteStringValue(id);json.WriteEndArray();}
        public string Encode()
        {
            Validate();using(var stream=new MemoryStream())
            {
                using(var json=new Utf8JsonWriter(stream,new JsonWriterOptions{Indented=true}))
                {
                    json.WriteStartObject();json.WriteNumber("version",Version);json.WriteString("template","settlement-1");
                    json.WriteString("id",Id);json.WriteString("name",Name);json.WriteString("base",BaseId);json.WriteString("worker",WorkerId);json.WriteString("workshop",WorkshopId);json.WriteNumber("supplies",Supplies);
                    json.WriteStartArray("units");foreach(var unit in Units)
                    {
                        json.WriteStartObject();json.WriteString("id",unit.Id);json.WriteString("name",unit.Name);json.WriteNumber("start",unit.Start);json.WriteBoolean("gathers",unit.Gathers);WriteLinks(json,"builds",unit.BuildIds);json.WriteEndObject();
                    }
                    json.WriteEndArray();json.WriteStartArray("buildings");foreach(var building in Buildings)
                    {
                        json.WriteStartObject();json.WriteString("id",building.Id);json.WriteString("name",building.Name);WriteLinks(json,"trains",building.Trains);json.WriteEndObject();
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
        private static string[] ReadLinks(JsonElement value)
        {
            var links=new List<string>();foreach(var entry in value.EnumerateArray())
            {if(links.Count>=8) throw new InvalidDataException("Too many blueprint links.");links.Add(entry.GetString());}return links.ToArray();
        }
        public static FactionRecord Decode(string text)
        {
            try
            {
                using(var document=JsonDocument.Parse(text,new JsonDocumentOptions{MaxDepth=8}))
                {
                    var root=document.RootElement;
                    if(root.ValueKind!=JsonValueKind.Object || !root.TryGetProperty("version",out var version)||!version.TryGetInt32(out var number)||number<1||number>Version)
                        throw new InvalidDataException("This faction uses a missing or unsupported file version. Its file has been left unchanged.");
                    CheckFields(root,number==1?LegacyFields:number==2?SecondFields:Fields);
                    if(root.GetProperty("template").GetString()!="settlement-1") throw new InvalidDataException("This faction needs an unsupported blueprint collection.");
                    var record=new FactionRecord{Id=root.GetProperty("id").GetString(),Name=root.GetProperty("name").GetString(),BaseId=root.GetProperty("base").GetString(),WorkerId=root.GetProperty("worker").GetString(),WorkshopId=root.GetProperty("workshop").GetString(),Supplies=root.GetProperty("supplies").GetInt32()};
                    if(number==1)
                    {
                        record.Units=new[]{new FactionUnitRecord{Id=record.WorkerId,Name="Worker",Start=root.GetProperty("workers").GetInt32(),Gathers=root.GetProperty("gathers").GetBoolean(),BuildIds=root.GetProperty("builds").GetBoolean()?new[]{record.WorkshopId}:Array.Empty<string>()}};
                    }
                    else
                    {
                        var list=new List<FactionUnitRecord>();foreach(var unit in root.GetProperty("units").EnumerateArray())
                        {
                            if(list.Count>=FactionDraft.MaxUnitBlueprints) throw new InvalidDataException("Too many unit blueprints.");CheckFields(unit,"id name start gathers builds");
                            list.Add(new FactionUnitRecord{Id=unit.GetProperty("id").GetString(),Name=unit.GetProperty("name").GetString(),Start=unit.GetProperty("start").GetInt32(),Gathers=unit.GetProperty("gathers").GetBoolean(),BuildIds=number==2?(unit.GetProperty("builds").GetBoolean()?new[]{record.WorkshopId}:Array.Empty<string>()):ReadLinks(unit.GetProperty("builds"))});
                        }
                        record.Units=list.ToArray();
                    }
                    if(number<3)
                    {
                        string trained=number==1?(root.GetProperty("trains").GetBoolean()?record.WorkerId:null):root.GetProperty("trained").GetString();
                        record.Buildings=new[]{new FactionBuildingRecord{Id=record.WorkshopId,Name="Workshop",Trains=trained!=null?new[]{trained}:Array.Empty<string>()}};
                    }
                    else
                    {
                        var list=new List<FactionBuildingRecord>();foreach(var building in root.GetProperty("buildings").EnumerateArray())
                        {
                            if(list.Count>=FactionDraft.MaxBuildingBlueprints) throw new InvalidDataException("Too many building blueprints.");CheckFields(building,"id name trains");
                            list.Add(new FactionBuildingRecord{Id=building.GetProperty("id").GetString(),Name=building.GetProperty("name").GetString(),Trains=ReadLinks(building.GetProperty("trains"))});
                        }
                        record.Buildings=list.ToArray();
                    }
                    record.Validate();return record;
                }
            }
            catch(Exception error) when(error is JsonException || error is InvalidOperationException || error is FormatException || error is OverflowException)
            {throw new InvalidDataException("This faction file is damaged or contains invalid values.",error);}
        }
        public FactionDraft CreateDraft(CivilizationDefinition template)
        {
            Validate();var draft=new FactionDraft(template);try
            {
                if(draft.Definition.StartingBase.Id!=BaseId || draft.TemplateWorkerId!=WorkerId || draft.TemplateWorkshopId!=WorkshopId) throw new InvalidDataException("One or more blueprint templates are unavailable in this version of the game.");
                draft.RestoreGraph(Units,Buildings);draft.SetFactionSetup(Name,Supplies);return draft;
            }
            catch{draft.Dispose();throw;}
        }
    }
}
