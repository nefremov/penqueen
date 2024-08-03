using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Penqueen.CodeGenerators.Proxies.Descriptors;

using System.Reflection;

using Xunit;

namespace Penqueen.Tests.Unit
{
    public class DbContextDescriptorFixtures
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void EqualsWhenDbContextIsSame(bool generateProxy, bool generateMixins)
        {
            Compilation inputCompilation = CreateCompilation("namespace MyCode{public class Program{public static void Main(string[] args){}}}");
            ITypeSymbol typeSymbol = inputCompilation.GetTypeByMetadataName("MyCode.Program")!;
            var entityDescriptions = new List<EntityDescriptor>();
            var one = new DbContextDescriptor(typeSymbol, true, false, entityDescriptions);
            var two = new DbContextDescriptor(typeSymbol, generateProxy, generateMixins, entityDescriptions);
            Assert.Equal(one, two);
        }

        [Fact]
        public void NotEqualsWhenDbContextsAreDifferent()
        {
            Compilation inputCompilation1 = CreateCompilation("namespace MyCode{public class Program{public static void Main(string[] args){}}}");
            Compilation inputCompilation2 = CreateCompilation("namespace MyCode{public class Program{public static void Main(string[] args){}}}");
            ITypeSymbol typeSymbol1 = inputCompilation1.GetTypeByMetadataName("MyCode.Program")!;
            ITypeSymbol typeSymbol2 = inputCompilation2.GetTypeByMetadataName("MyCode.Program")!;
            var entityDescriptions = new List<EntityDescriptor>();
            var one = new DbContextDescriptor(typeSymbol1, true, true, entityDescriptions);
            var two = new DbContextDescriptor(typeSymbol2, true, true, entityDescriptions);
            Assert.NotEqual(one, two);
        }


        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}
