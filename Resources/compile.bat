PATH = %PATH%;c:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\;

resgen /str:c#,Resources /publicClass /compile Shared.txt,Shared.resx
resgen /compile Shared.ru.txt,Shared.ru.resx

resgen /str:c#,Resources /publicClass /compile Article.txt,Article.resx
resgen /compile Article.ru.txt,Article.ru.resx