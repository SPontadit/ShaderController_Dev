using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEditor;

namespace Assets.Editor
{
    /// <summary>Editor post processor who will related the T4 template to the generated file</summary>
    /// <remarks>Need to put this file in an editor subfolder to make sure Unity will pick it up</remarks>
    /// <seealso href="https://forum.unity.com/threads/editor-call-back-after-generate-visual-studio-files.508061/"/>
    public sealed class T4TemplatePostProcessor : AssetPostprocessor
    {
        private static readonly Regex CompileFileRegex = new Regex(@"^.*<Compile Include=""(.*)"".*/>.*$");

        private static bool HasTemplate(string fileName, out string templateName)
        {
            var directory = Path.GetDirectoryName(fileName) ?? "";
            var bareFileName = Path.GetFileNameWithoutExtension(fileName) ?? fileName;
            if (bareFileName.EndsWith(".g"))
            {
                bareFileName = Path.GetFileNameWithoutExtension(fileName);
            }

            templateName = Path.Combine(directory, bareFileName + ".tt");

            return File.Exists(templateName);
        }

        private static void WriteGeneratedFileData(TextWriter writer, string fileName, string ttFileName)
        {
            writer.WriteLine($@"    <!-- Begin adding T4 for {fileName} -->                             ");
            writer.WriteLine($@"    <None Include=""{ttFileName}"">                                     ");
            writer.WriteLine($@"        <Generator>TextTemplatingFilePreprocessor</Generator>           ");
            writer.WriteLine($@"        <LastGenOutput>{Path.GetFileName(fileName)}</LastGenOutput>     ");
            writer.WriteLine($@"    </None>                                                             ");
            writer.WriteLine($@"    <Compile Include=""{fileName}"">                                    ");
            writer.WriteLine($@"        <AutoGen>True</AutoGen>                                         ");
            writer.WriteLine($@"        <DesignTime>True</DesignTime>                                   ");
            writer.WriteLine($@"        <DependentUpon>{Path.GetFileName(ttFileName)}</DependentUpon>   ");
            writer.WriteLine($@"    </Compile>                                                          ");
            writer.WriteLine($@"    <!-- End adding T4 for {fileName} -->                               ");
        }

        /// <summary>Method automatically called upon CS project generation</summary>
        [MenuItem("Assets /Force T4 Detection", priority = 1000)]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public static void OnGeneratedCSProjectFiles()
        {
            string[] projectFiles;
            try
            {
                projectFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), @"Assembly-CSharp-Editor.csproj", SearchOption.TopDirectoryOnly);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                UnityEngine.Debug.LogError("Failed processing csproj for T4 templates");
                return;
            }

            foreach (var file in projectFiles)
            {
                var modified = false;

                var tempProjectFile = FileUtil.GetUniqueTempPathInProject();
                using (var reader = new StreamReader(file))
                using (var writer = new StreamWriter(tempProjectFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var match = CompileFileRegex.Match(line);
                        if (!match.Success || !HasTemplate(match.Groups[1].Value, out var template))
                        {
                            writer.WriteLine(line);
                            continue;
                        }

                        modified = true;
                        WriteGeneratedFileData(writer, match.Groups[1].Value, template);
                    }
                }

                if (modified)
                {
                    File.Copy(tempProjectFile, file, true);
                }
                File.Delete(tempProjectFile);
            }
        }
    }
}