namespace WrapInRegions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Microsoft.CodeAnalysis.Formatting;

  internal sealed class WrapInRegionsRefactoringCodeFixAction
    {
        public ClassDeclarationSyntax Apply(ClassDeclarationSyntax typeDeclaration)
        {
            var membersToCategorize = typeDeclaration.Members.ToList();

            var ws = Enumerable.Empty<MemberDeclarationSyntax>()
              .Concat(this.CreateRegionForMemberCategory(membersToCategorize, "Members", IsField))
              .Concat(this.CreateRegionForMemberCategory(membersToCategorize, "Construction", m => IsConstructor(m) || IsFactoryMethod(m, typeDeclaration)))
              .Concat(this.CreateRegionForMemberCategory(membersToCategorize, "Properties", IsProperty))
              .Concat(this.CreateRegionForMemberCategory(membersToCategorize, "Methods", IsMethod));

            return typeDeclaration
              .WithMembers(new SyntaxList<MemberDeclarationSyntax>(ws))
              .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private IEnumerable<MemberDeclarationSyntax> CreateRegionForMemberCategory(ICollection<MemberDeclarationSyntax> allMembers, string regionName, Func<MemberDeclarationSyntax, bool> selector)
        {
            var affectedMembers = allMembers.Where(selector).ToList();

            var newMemberList = new List<MemberDeclarationSyntax>();

            foreach (var currentMember in affectedMembers)
            {
                var newMember = currentMember;

                allMembers.Remove(currentMember);

                if (currentMember == affectedMembers.First())
                {
                    newMember = newMember.WithLeadingTrivia(
                      CreateRegionStart(regionName));
                }

                if (currentMember == affectedMembers.Last())
                {
                    newMember = newMember.WithTrailingTrivia(
                      SyntaxFactory.CarriageReturnLineFeed,
                      CreateRegionEnd(regionName),
                      SyntaxFactory.CarriageReturnLineFeed);
                }

                newMemberList.Add(newMember);
            }

            return newMemberList;
        }

        private static bool IsField(MemberDeclarationSyntax member) => member is FieldDeclarationSyntax;

        private static bool IsConstructor(MemberDeclarationSyntax member)
        {
            return member is ConstructorDeclarationSyntax;
        }

        private static bool IsFactoryMethod(MemberDeclarationSyntax member, ClassDeclarationSyntax typeDeclaration)
        {
            if (member is MethodDeclarationSyntax method)
            {
                if (method.Modifiers.Any(SyntaxKind.StaticKeyword) && method.ReturnType is IdentifierNameSyntax returnTypeIdentifier)
                {
                    return returnTypeIdentifier.Identifier.Text == typeDeclaration.Identifier.Text;
                }
            }

            return false;
        }

        private static bool IsProperty(MemberDeclarationSyntax member) => member is PropertyDeclarationSyntax;

        private static bool IsMethod(MemberDeclarationSyntax member) => member is MethodDeclarationSyntax;

        private static SyntaxTrivia CreateRegionStart(string message)
        {
            return SyntaxFactory.Trivia(
              SyntaxFactory.RegionDirectiveTrivia(
                  true)
                .WithRegionKeyword(
                  SyntaxFactory.Token(
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.RegionKeyword,
                    SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                .WithEndOfDirectiveToken(
                  SyntaxFactory.Token(
                    SyntaxFactory.TriviaList(
                      SyntaxFactory.PreprocessingMessage(message)),
                    SyntaxKind.EndOfDirectiveToken,
                    SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed))));
        }

        private static SyntaxTrivia CreateRegionEnd(string message)
        {
            return SyntaxFactory.Trivia(
              SyntaxFactory.EndRegionDirectiveTrivia(
                  true)
                .WithEndRegionKeyword(
                  SyntaxFactory.Token(
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.EndRegionKeyword,
                    SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                .WithEndOfDirectiveToken(
                  SyntaxFactory.Token(
                    SyntaxFactory.TriviaList(
                      SyntaxFactory.PreprocessingMessage(message)),
                    SyntaxKind.EndOfDirectiveToken,
                    SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed))));
        }
    }
}
