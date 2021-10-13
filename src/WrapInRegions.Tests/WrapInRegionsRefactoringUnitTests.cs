using VerifyCS = WrapInRegions.Verifiers.CSharpCodeFixVerifier<
    WrapInRegions.WrapInRegionsRefactoringAnalyzer,
    WrapInRegions.WrapInRegionsRefactoringCodeFixProvider>;

namespace WrapInRegions
{
  using System.Threading.Tasks;
  using Microsoft.CodeAnalysis.Testing;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
    public class WrapInRegionsRefactoringUnitTest
    {
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
          public static TypeName New() { return new TypeName(); }
          public void A() {}
          public void B() {}
          public void C() {}
          private readonly string field;
          public TypeName() {}
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
    class TypeName
    {
        #region Members
        private readonly string field;
        #endregion Members

        #region Construction
        public static TypeName New() { return new TypeName(); }
        public TypeName() { }
        #endregion Construction

        #region Methods
        public void A() { }
        public void B() { }
        public void C() { }
        #endregion Methods

    }
}";

            var expectedDiagnostics = VerifyCS.Diagnostic(Constants.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.IgnoreLength).WithArguments("TypeName");
            await VerifyCS.VerifyCodeFixAsync(test, expectedDiagnostics, fixtest);
        }
    }
}
