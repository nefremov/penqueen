using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using System.Reflection;
using System.Text;
using Penqueen.Types;
using Xunit;

using VerifyCS = Penqueen.Tests.Unit.GeneratorVerifier<Penqueen.CodeGenerators.Entities.EntityClassGenerator>;
namespace Penqueen.Tests.Unit;


public class PartialClassTest
{
    [Fact]
    public async void PartialWithCollections()
    {
        var code = """
                   using Penqueen.Types;
                   
                   namespace EntSpace;
                   [Entity]
                   public partial class Ent1 {
                       public virtual ICollection<Ref1> Refs { get; }
                       
                       public Ent1 (int i1, int? i2) {}
                   }                  

                   public class Ref1 {
                   }                  
                   """;
        var generatedPartial = """
                               using System.Collections.Generic;
                               
                               namespace EntSpace;
                               
                               public partial class Ent1
                               {
                                   protected ICollection<EntSpace.Ref1>? _refs;
                               }
                               
                               """;
        var generatedPartial2 = """
                               using System.Collections.Generic;

                               namespace EntSpace;

                               public partial class Ref1
                               {
                               }
                               
                               """;
        var generatedCollection = """
                                  using Penqueen.Collections;

                                  namespace EntSpace;

                                  public interface IEnt1Collection : IQueryableCollection<Ent1>
                                  {
                                      Ent1 CreateNew
                                      (
                                          int i1,
                                          int? i2
                                      );
                                  }
                                  
                                  """;
        var generatedCollection2 = """
                                  using Penqueen.Collections;

                                  namespace EntSpace;

                                  public interface IRef1Collection : IQueryableCollection<Ref1>
                                  {
                                      Ref1 CreateNew
                                      (
                                          bool b1,
                                          bool? b2
                                      );
                                  }
                                  """;
        //var asms = AppDomain.CurrentDomain.GetAssemblies()
        //    .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location)).ToList();
        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(System.Boolean).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(EntityAttribute).GetTypeInfo().Assembly.Location),
                },
                GeneratedSources =
                {
                    (typeof(Penqueen.CodeGenerators.Entities.EntityClassGenerator), "Ent1.g.cs", SourceText.From(generatedPartial, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                    //(typeof(Penqueen.CodeGenerators.Entities.EntityClassGenerator), "Ref1.g.cs", SourceText.From(generatedPartial2, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                    (typeof(Penqueen.CodeGenerators.Entities.EntityClassGenerator), "IEnt1Collection.g.cs", SourceText.From(generatedCollection, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                    //(typeof(Penqueen.CodeGenerators.Entities.EntityClassGenerator), "IRef1Collection.g.cs", SourceText.From(generatedCollection2, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }
}