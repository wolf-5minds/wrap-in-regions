namespace WrapInRegions
{
  using System.Collections.Immutable;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Microsoft.CodeAnalysis.Diagnostics;

  [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WrapInRegionsRefactoringAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(Constants.DiagnosticId, Title, MessageFormat, Constants.DiagnosticCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(this.AnalyzeType, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax typeDeclaration))
            {
                return;
            }


            var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            var isMissingRegion = typeDeclaration.ToFullString().Contains("#region") == false;
            //var isMissingRegion = typeDeclaration.Accept(new HasAnyRegionSyntaxVisitor()) == false;

            if (isMissingRegion)
            {
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private sealed class HasAnyRegionSyntaxVisitor : CSharpSyntaxVisitor<bool>
        {
          public override bool VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
          {
            return true;
            //base.VisitRegionDirectiveTrivia(node);
          }
        }
    }


}
