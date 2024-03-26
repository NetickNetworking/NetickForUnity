using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using Netick.CodeGen;
using Unity.Netick.CodeGen;
using ILPPInterface = Unity.CompilationPipeline.Common.ILPostProcessing.ILPostProcessor;

namespace Unity.Netick.Helper.CodeGen
{
  public sealed class CodeGen : ILPPInterface
  {
    private NetickILProcessor                _netickILProcessor = new NetickILProcessor();
    private List<DiagnosticMessage>          _diagnostics = new List<DiagnosticMessage>();
    public override ILPPInterface            GetInstance() => this;
    public override bool                     WillProcess(ICompiledAssembly compiledAssembly) => compiledAssembly.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == CodeGenUtils.RuntimeAssemblyName);

    public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
    {
      var assemblyDefinition = CodeGenUtils.AssemblyDefinitionFor(compiledAssembly, out var unused);
      if (assemblyDefinition == null)
      {
        _diagnostics.AddError($"Cannot read Netick Runtime assembly definition: {compiledAssembly.Name}");
        return null;
      }

      if (!Weave(assemblyDefinition))
        return null;

      var pe  = new MemoryStream();
      var pdb = new MemoryStream();

      var writerParameters = new WriterParameters
      {
        SymbolWriterProvider = new PortablePdbWriterProvider(),
        SymbolStream         = pdb,
        WriteSymbols         = true
      };

      assemblyDefinition.Write(pe, writerParameters);
      return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), _diagnostics);
    }

    public bool Weave(AssemblyDefinition assemblyDefinition)
    {
      var mainModule = assemblyDefinition.MainModule;
      var asm        = assemblyDefinition.MainModule.Name.Replace(".dll", "");

      if (asm == "Netick" || asm == "Netick.Unity")
          return false;

      var didProcess = false;

      if (mainModule != null)
      {
        var weaver = new UnityCodeGen();
        weaver.Init(mainModule, _diagnostics);
        _netickILProcessor.Init(weaver, mainModule);
        didProcess = _netickILProcessor.ProcessAssembly(assemblyDefinition);
      }
      else
        _diagnostics.AddError($"Cannot get main module from assembly definition: {assemblyDefinition.Name}");
      return didProcess;
    }
  }
}
