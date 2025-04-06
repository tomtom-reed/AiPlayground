using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;

namespace AiPlayground.Services.AiServices;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class OpenAIAssistantBuilder
{
    private readonly OpenAIClient _client;

    // Base model comes from https://platform.openai.com/docs/models
    // eg gpt-4o-mini
    private readonly string _baseModel;
    private string _name = "";
    private string _description = "";
    private string _instructions = "";
    private float? _temperature;
    private readonly IEnumerable<MyVectorStore> _files = new List<MyVectorStore>();
    private struct MyVectorStore
    {
        public string Name {get; set;}
        public Stream File {get; set;}
    }
    private AssistantResponseFormat? _format;
    
    public OpenAIAssistantBuilder(string baseModel) {
        _client = new(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        _baseModel = baseModel;
    }

    public Assistant Build()
    {
        var options = new AssistantCreationOptions() 
        {
            Name = _name,
            Description = _description,
            Instructions = _instructions,
            Temperature = _temperature ?? 1.0f,
            ResponseFormat = _format ?? null, // might work?
        };
        if (_files.Count() > 0)
        {
            // ref https://platform.openai.com/docs/assistants/tools/file-search

            // add file search tool
            options.Tools.Add(ToolDefinition.CreateFileSearch());
            
            // create vector store
            var vsc = _client.GetVectorStoreClient();
            var vo = new VectorStoreCreationOptions();
            vo.ChunkingStrategy = FileChunkingStrategy.Auto;
            vo.ExpirationPolicy = new VectorStoreExpirationPolicy(VectorStoreExpirationAnchor.LastActiveAt, 90);
            var vs = vsc.CreateVectorStore(false);//, vectorStore: vo);
            
            // Add files to the vector store
            var fc = _client.GetOpenAIFileClient();
            foreach (var f in _files)
            {
                OpenAIFile fupload = fc.UploadFile(f.File, f.Name, OpenAI.Files.FileUploadPurpose.Batch);
                vsc.AddFileToVectorStore(vs.VectorStoreId, fupload.Id, false);
            }

            // add Vector Store to Options
            options.ToolResources.FileSearch.VectorStoreIds.Add(vs.VectorStoreId);
        }
        var assist = _client.GetAssistantClient().CreateAssistant(_name, options);
        
        return assist.Value;
    }

    /// <summary>
    /// The assistant name is documentation that is not used by the assistant directly.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public OpenAIAssistantBuilder AddName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// The assistant description is documentation that is not used by the assistant directly.
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public OpenAIAssistantBuilder AddDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// System Instructions are the primary training source for the assistant.
    //  Consider it similar to a job description: how does the assistant act and what will it be used for.
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    public OpenAIAssistantBuilder AddSystemInstructions(string instructions) 
    {
        _instructions = instructions;
        return this;
    }

    /// <summary>
    /// Training files are knowledge bases available to the assistant which give it more information to work from than the foundation GPT model is usually aware of.
    /// Examples could include stock ticker symbols, glossaries of esoteric or internal terms, or a collection of blog posts by a certain author.
    /// 
    /// Optional.
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public OpenAIAssistantBuilder AddTrainingFile(string fileId, Stream file)
    {
        var v = new MyVectorStore();
        v.Name = fileId;
        v.File = file;
        _files.Append(v);
        return this;
    }

    /// <summary>
    /// Temperature is a measure of how predictable the assistant's results will be.
    /// It is on a scale of 0.0f to 2.0f inclusive. Lower values are more deterministic.
    /// 
    /// Optional. The default value is 1.0f.
    /// </summary>
    /// <param name="temp"></param>
    /// <returns></returns>
    public OpenAIAssistantBuilder SetTemperature(float temp=1.0f)
    {
        _temperature = temp;
        return this;
    }

    /// <summary>
    /// Pass a type and the assistant will attempt to format its responses into the provided type.
    /// The internals used are json serialization/deserialization. 
    /// 
    /// ref https://platform.openai.com/docs/guides/structured-outputs?api-mode=responses 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="jsonSerializedObject"></param>
    /// <returns></returns>
    public OpenAIAssistantBuilder AddResponseFormat(string name, Type template)
    {
        var options = JsonSerializerOptions.Default;
        JsonSchemaExporterOptions exporterOptions = new()
        {
            TreatNullObliviousAsNonNullable = true,
        };

        var schema = options.GetJsonSchemaAsNode(template, exporterOptions);

        _format = AssistantResponseFormat.CreateJsonSchemaFormat(name, BinaryData.FromString(schema.ToString()), strictSchemaEnabled: true);
        return this;
    }

    // Assistants also support "Function" and "File Search" tools
    // But my code only supports "Vector Store" aka file training data tooling for now

    
}
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
