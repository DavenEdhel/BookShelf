using System.Text;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2TemplateString : IString
    {
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public IBase64 Cover { get; set; } = new EmptyBase64();

        public string Annotation { get; set; } = string.Empty;

        public string Value => @"<?xml version=""1.0"" encoding=""utf-8""?>
<FictionBook xmlns:l=""http://www.w3.org/1999/xlink"" xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
  <description>
    <title-info>
      <genre>fantasy</genre>
      <author>
        <first-name>Станислав</first-name>
        <last-name>Конопляник</last-name>
        <home-page>https://author.today/u/daven_edhel</home-page>
      </author>
      <book-title>$title</book-title>
      <annotation>
        <p>$annotation</p>
      </annotation>
      <date value=""2020-08-09"">2020-08-09 06:29:06</date>
      <coverpage>
        <image l:href=""#cover.jpg"" alt=""Обложка"" />
      </coverpage>
      <lang>ru</lang>
    </title-info>
    <document-info>
      <author>
        <first-name>Станислав</first-name>
        <last-name>Конопляник</last-name>
        <home-page>https://author.today/u/daven_edhel</home-page>
        <id>59406</id>
      </author>
      <date value =""2020-08-09"">2020-08-09 06:29:06</date>
      <id>70933</id>
      <version>1.00</version>
    </document-info>
    <publish-info />
  </description>
  <body>
    <title>$title</title>
    $body
  </body>
  <binary content-type=""image/jpg"" id=""cover.jpg"">$cover</binary>
</FictionBook>
".Replace("$title", Title).Replace("$annotation", Annotation).Replace("$cover", Cover.Value).Replace("$body", Body);
    }
}