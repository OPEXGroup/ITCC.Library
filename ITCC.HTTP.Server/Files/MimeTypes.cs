﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;

namespace ITCC.HTTP.Server.Files
{
    public static class MimeTypes
    {
        public static string GetTypeByExtenstion(string extension)
        {
            if (ExtensionTypeDictionary.ContainsKey(extension))
                return ExtensionTypeDictionary[extension];
            return $"x-application/{extension}";
        }

        private static readonly Dictionary<string, string> ExtensionTypeDictionary = new Dictionary<string, string>
        {
            {"ai", "application/postscript"},
            {"aif", "audio/aiff"},
            {"aiff", "audio/aiff"},
            {"ani", "application/x-navi-animation"},
            {"aos", "application/x-nokia-9000-communicator-add-on-software"},
            {"aps", "application/mime"},
            {"arc", "application/octet-stream"},
            {"arj", "application/arj"},
            {"art", "image/x-jg"},
            {"asf", "video/x-ms-asf"},
            {"asm", "text/x-asm"},
            {"asp", "text/asp"},
            {"asx", "application/x-mplayer2"},
            {"au", "audio/basic"},
            {"avi", "application/x-troff-msvideo"},
            {"bin", "application/mac-binary"},
            {"bm", "image/bmp"},
            {"bmp", "image/bmp"},
            {"boo", "application/book"},
            {"book", "application/book"},
            {"c", "text/x-c"},
            {"c++", "text/x-cpp"},
            {"ccad", "application/clariscad"},
            {"class", "application/java"},
            {"com", "application/octet-stream"},
            {"conf", "text/plain"},
            {"cpp", "text/x-c"},
            {"cpt", "application/mac-compactpro"},
            {"css", "application/x-pointplus"},
            {"dcr", "application/x-director"},
            {"def", "text/plain"},
            {"dif", "video/x-dv"},
            {"dir", "application/x-director"},
            {"dl", "video/dl"},
            {"doc", "application/msword"},
            {"dot", "application/msword"},
            {"drw", "application/drafting"},
            {"dvi", "application/x-dvi"},
            {"dwg", "application/acad"},
            {"dxf", "application/dxf"},
            {"dxr", "application/x-director"},
            {"exe", "application/octet-stream"},
            {"gif", "image/gif"},
            {"gz", "application/x-compressed"},
            {"gzip", "application/x-gzip"},
            {"h", "text/plain"},
            {"hlp", "application/hlp"},
            {"htc", "text/x-component"},
            {"htm", "text/html"},
            {"html", "text/html"},
            {"htmls", "text/html"},
            {"htt", "text/webviewhtml"},
            {"ice", "x-conference/x-cooltalk"},
            {"ico", "image/x-icon"},
            {"inf", "application/inf"},
            {"jam", "audio/x-jam"},
            {"jav", "text/plain"},
            {"java", "text/plain"},
            {"jcm", "application/x-java-commerce"},
            {"jfif", "image/jpeg"},
            {"jfif-tbnl", "image/jpeg"},
            {"jpe", "image/jpeg"},
            {"jpeg", "image/jpeg"},
            {"jpg", "image/jpeg"},
            {"jps", "image/x-jps"},
            {"js", "application/x-javascript"},
            {"latex", "application/x-latex"},
            {"lha", "application/lha"},
            {"lhx", "application/octet-stream"},
            {"list", "text/plain"},
            {"lsp", "application/x-lisp"},
            {"lst", "text/plain"},
            {"lzh", "application/octet-stream"},
            {"lzx", "application/lzx"},
            {"m3u", "audio/x-mpequrl"},
            {"man", "application/x-troff-man"},
            {"mid", "application/x-midi"},
            {"midi", "application/x-midi"},
            {"mod", "audio/mod"},
            {"mov", "video/quicktime"},
            {"movie", "video/x-sgi-movie"},
            {"mp2", "audio/mpeg"},
            {"mp3", "audio/mp3"},
            {"mp4", "video/mp4"},
            {"mpa", "audio/mpeg"},
            {"mpeg", "video/mpeg"},
            {"mpg", "audio/mpeg"},
            {"mpga", "audio/mpeg"},
            {"pas", "text/pascal"},
            {"pcl", "application/vnd.hp-pcl"},
            {"pct", "image/x-pict"},
            {"pcx", "image/x-pcx"},
            {"pdf", "application/pdf"},
            {"pic", "image/pict"},
            {"pict", "image/pict"},
            {"pl", "text/plain"},
            {"pm", "image/x-xpixmap"},
            {"pm4", "application/x-pagemaker"},
            {"pm5", "application/x-pagemaker"},
            {"png", "image/png"},
            {"pot", "application/mspowerpoint"},
            {"ppa", "application/vnd.ms-powerpoint"},
            {"pps", "application/mspowerpoint"},
            {"ppt", "application/mspowerpoint"},
            {"ppz", "application/mspowerpoint"},
            {"ps", "application/postscript"},
            {"psd", "application/octet-stream"},
            {"pwz", "application/vnd.ms-powerpoint"},
            {"py", "text/x-script.phyton"},
            {"pyc", "applicaiton/x-bytecode.python"},
            {"qt", "video/quicktime"},
            {"qtif", "image/x-quicktime"},
            {"ra", "audio/x-pn-realaudio"},
            {"ram", "audio/x-pn-realaudio"},
            {"rm", "application/vnd.rn-realmedia"},
            {"rpm", "audio/x-pn-realaudio-plugin"},
            {"rtf", "application/rtf"},
            {"rtx", "application/rtf"},
            {"rv", "video/vnd.rn-realvideo"},
            {"sgml", "text/sgml"},
            {"sh", "application/x-bsh"},
            {"shtml", "text/html"},
            {"ssi", "text/x-server-parsed-html"},
            {"tar", "application/x-tar"},
            {"tcl", "application/x-tcl"},
            {"text", "application/plain"},
            {"tgz", "application/gnutar"},
            {"tif", "image/tiff"},
            {"tiff", "image/tiff"},
            {"txt", "text/plain"},
            {"uri", "text/uri-list"},
            {"vcd", "application/x-cdlink"},
            {"vmd", "application/vocaltec-media-desc"},
            {"vrml", "application/x-vrml"},
            {"vsd", "application/x-visio"},
            {"vst", "application/x-visio"},
            {"vsw", "application/x-visio"},
            {"wav", "audio/wav"},
            {"wmf", "windows/metafile"},
            {"xla", "application/excel"},
            {"xlb", "application/excel"},
            {"xlc", "application/excel"},
            {"xld", "application/excel"},
            {"xlk", "application/excel"},
            {"xll", "application/excel"},
            {"xlm", "application/excel"},
            {"xls", "application/excel"},
            {"xlt", "application/excel"},
            {"xlv", "application/excel"},
            {"xlw", "application/excel"},
            {"xm", "audio/xm"},
            {"xml", "application/xml"},
            {"z", "application/x-compress"},
            {"zip", "application/x-compressed"}
        };
    }
}
