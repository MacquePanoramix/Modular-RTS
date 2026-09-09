using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
namespace WonderGather
{
    // Version 1 represents the first creator's exact, deliberately small content contract.
    public sealed class FactionRecord
    {
        public const int Version=1;
        public string Id,Name,BaseId,WorkerId,WorkshopId;
        public int Workers,Supplies;
        public bool Gathers,Builds,Trains;
        private static readonly HashSet<string> Fields=new HashSet<string>{"version","template","id","name","base","worker","workshop","workers","supplies","gathers","builds","trains"};
        public static FactionRecord Capture(FactionDraft draft,string id)
        {
            return new FactionRecord{Id=id,Name=draft.Definition.DisplayName,BaseId=draft.Definition.StartingBase.Id,
                WorkerId=draft.Worker.Id,WorkshopId=draft.Workshop.Id,Workers=draft.StartingWorkers,Supplies=draft.Definition.StartingSupplies,
                Gathers=draft.Worker.GathersSupplies,Builds=draft.Worker.CanBuild(draft.Workshop),Trains=draft.Workshop.Produces!=null};
        }
        public void Validate()
        {
            if(!Guid.TryParseExact(Id,"N",out _)) throw new InvalidDataException("The faction file has an invalid ID.");
            if(string.IsNullOrWhiteSpace(Name) || Name.Length>64 || Name.Any(char.IsControl)) throw new InvalidDataException("Faction names need 1–64 printable characters.");
            if(Workers<0 || Workers>8 || Supplies<0 || Supplies>120) throw new InvalidDataException("Starting values exceed this prototype's supported limits.");
            if(string.IsNullOrWhiteSpace(BaseId)||string.IsNullOrWhiteSpace(WorkerId)||string.IsNullOrWhiteSpace(WorkshopId)) throw new InvalidDataException("Blueprint IDs are missing.");
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
                    json.WriteNumber("workers",Workers);json.WriteNumber("supplies",Supplies);json.WriteBoolean("gathers",Gathers);json.WriteBoolean("builds",Builds);json.WriteBoolean("trains",Trains);
                    json.WriteEndObject();
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        public static FactionRecord Decode(string text)
        {
            try
            {
                using(var document=JsonDocument.Parse(text,new JsonDocumentOptions{MaxDepth=8}))
                {
                    var root=document.RootElement;
                    if(root.ValueKind!=JsonValueKind.Object) throw new InvalidDataException("Expected a faction document.");
                    if(!root.TryGetProperty("version",out var version)||!version.TryGetInt32(out var number)||number!=Version)
                        throw new InvalidDataException("This faction uses a missing or unsupported file version. Its file has been left unchanged.");
                    var seen=new HashSet<string>();
                    foreach(var field in root.EnumerateObject())
                        if(!Fields.Contains(field.Name)||!seen.Add(field.Name)) throw new InvalidDataException("This faction contains unknown or duplicate fields; it will not be rewritten.");
                    if(seen.Count!=Fields.Count) throw new InvalidDataException("This faction file is incomplete.");
                    if(root.GetProperty("template").GetString()!="settlement-1") throw new InvalidDataException("This faction needs an unsupported blueprint collection.");
                    var record=new FactionRecord{Id=root.GetProperty("id").GetString(),Name=root.GetProperty("name").GetString(),BaseId=root.GetProperty("base").GetString(),WorkerId=root.GetProperty("worker").GetString(),WorkshopId=root.GetProperty("workshop").GetString(),Workers=root.GetProperty("workers").GetInt32(),Supplies=root.GetProperty("supplies").GetInt32(),Gathers=root.GetProperty("gathers").GetBoolean(),Builds=root.GetProperty("builds").GetBoolean(),Trains=root.GetProperty("trains").GetBoolean()};
                    record.Validate();return record;
                }
            }
            catch(Exception error) when(error is JsonException || error is InvalidOperationException || error is FormatException || error is OverflowException)
            {throw new InvalidDataException("This faction file is damaged or contains invalid values.",error);}
        }
        public FactionDraft CreateDraft(CivilizationDefinition template)
        {
            Validate();var draft=new FactionDraft(template);
            if(draft.Definition.StartingBase.Id!=BaseId || draft.Worker.Id!=WorkerId || draft.Workshop.Id!=WorkshopId)
            {draft.Dispose();throw new InvalidDataException("One or more blueprint IDs are unavailable in this version of the game.");}
            draft.SetStartingSetup(Name,Workers,Supplies);draft.SetWorkerPermissions(Gathers,Builds);draft.SetProduction(Trains);return draft;
        }
    }
}
