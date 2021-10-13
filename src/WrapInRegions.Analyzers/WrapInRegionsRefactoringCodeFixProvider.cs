namespace WrapInRegions
{
  using System.Collections.Immutable;
  using System.Composition;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CodeActions;
  using Microsoft.CodeAnalysis.CodeFixes;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WrapInRegionsRefactoringCodeFixProvider)), Shared]
    public sealed class WrapInRegionsRefactoringCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Constants.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var typeDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Constants.CodeFixName,
                    createChangedSolution: c => this.ApplyWrappingInRegionsAsync(context.Document, typeDeclaration, c),
                    equivalenceKey: Constants.CodeFixName),
                diagnostic);
        }

        private async Task<Solution> ApplyWrappingInRegionsAsync(Document document, ClassDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var action = new WrapInRegionsRefactoringCodeFixAction();

            var fixedCode = action.Apply(typeDeclaration);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(typeDeclaration, fixedCode);

            return solution.WithDocumentSyntaxRoot(document.Id, newRoot);
        }
    }
}
