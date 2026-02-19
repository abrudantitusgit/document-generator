using System.IO;
using Microsoft.AspNetCore.Mvc;
using Xceed.Words.NET;

public class WordController : Controller
{
    [HttpPost("test")]
    public IActionResult ReplacePlaceholder()
    {
        string filePath = @"D:\test.docx";
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return BadRequest("Please provide a valid Word file path.");

        using (var document = DocX.Load(filePath))
        {
            // Replace the placeholder text
            string placeholder = "{Timi}";
            string replacementText = "random";

            foreach (var paragraph in document.Paragraphs)
            {
                paragraph.ReplaceText(placeholder, replacementText);
            }

            // Save the modified document
            document.Save();

            return Ok(new { ModifiedFilePath = filePath });
        }
    }
}