/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2015
 * 
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

/*
 * Description:
 *     add/remove header to source code recursively
 *
 * Revision history:
 *     2015-03-09, @imzhenyu (Zhenyu Guo), first version
 *     xxxx-xx-xx, author, fix bug about xxx
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace code.header
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("Usage: code.header header_file input_dir output_dir add|remove ext1 ext2 ... (e.g., .cpp .h .hpp .cc)");
        }

        public class Options
        {
            public string headerFile;
            public string headerContent;
            public string inputDir;
            public string outputDir;
            public string action;
            public bool IsRemove;
            public bool IsAdd;
            public HashSet<string> fileExts = new HashSet<string>();

            public bool Parse(string[] args)
            {
                if (args.Length < 5)
                {
                    Usage();
                    return false;
                }

                headerFile = args[0];
                inputDir = args[1];
                outputDir = args[2];
                action = args[3];
                
                for (int i = 4; i < args.Length; i++)
                {
                    var s1 = args[i].Trim();
                    if (!fileExts.Contains(s1))
                    { 
                        fileExts.Add(s1);
                        Console.WriteLine("Files with extension '" + s1 + "' is supported.");
                    }
                }
                return true;
            }

            public bool Validate()
            {
                if (!File.Exists(headerFile))
                {
                    Console.WriteLine("Cannot find input header file '" + headerFile + "'");
                    return false;
                }

                if (!Directory.Exists(inputDir))
                {
                    Console.WriteLine("Cannot find input directory '" + inputDir + "'");
                    return false;
                }

                if (Directory.Exists(outputDir))
                {
                    Console.WriteLine("Output directory '" + outputDir + "' already exists, must use new output directory to avoid accident!");
                    return false;
                }

                IsRemove = (action == "remove" || action == "REMOVE");
                IsAdd = (action == "add" || action == "ADD");
                if (!IsRemove && !IsAdd)
                {
                    Console.WriteLine("unsupported action '" + action + "'");
                    return false;
                }

                headerContent = File.ReadAllText(headerFile);
                Console.WriteLine("---- THE FOLLOWING CONTENT WILL BE ADDED TO THE FILES ------");
                Console.Write(headerContent);
                Console.WriteLine();
                Console.WriteLine("------------------------------------------------------------");

                return true;
            }
        }

        static void RunDir(string sdir, string ddir, Options opts)
        {
            if (!Directory.Exists(sdir))
                throw new Exception("Source dir '" + sdir + "' not exits");

            if (Directory.Exists(ddir))
                throw new Exception("Destination dir '" + sdir + "' already exits");

            Directory.CreateDirectory(ddir);
            foreach (var f in Directory.GetFiles(sdir))
            {
                var f1 = f;
                var idx = f1.LastIndexOfAny(new char[]{'/', '\\'});
                if (idx != -1)
                {
                    f1 = f1.Substring(idx + 1);
                }

                RunFile(Path.Combine(sdir, f1), Path.Combine(ddir, f1), opts);
            }

            foreach (var f in Directory.GetDirectories(sdir))
            {
                var f1 = f;
                var idx = f1.LastIndexOfAny(new char[] { '/', '\\' });
                if (idx != -1)
                {
                    f1 = f1.Substring(idx + 1);
                }

                if (f1 == "." || f1 == "..")
                    continue;

                RunDir(Path.Combine(sdir, f1), Path.Combine(ddir, f1), opts);
            }
        }

        static void RunFile(string sfile, string dfile, Options opts)
        {
            if (!File.Exists(sfile))
                throw new Exception("Source file '" + sfile + "' not exits");

            if (File.Exists(dfile))
                throw new Exception("Destination file '" + dfile + "' already exits");

            var idx = sfile.LastIndexOf('.');
            if (idx != -1)
            {
                var ext = sfile.Substring(idx);
                if (!opts.fileExts.Contains(ext))
                    idx = -1;
            }

            if (idx == -1)
            {
                File.Copy(sfile, dfile);
                Console.WriteLine("Copy file to '" + dfile + "'");
                return;
            }

            var content = File.ReadAllText(sfile);

            if (opts.IsAdd)
            {
                content = opts.headerContent + content;
                Console.WriteLine("Add header to file '" + dfile  + "'");
            }
            else if (opts.IsRemove)
            {
                if (content.StartsWith(opts.headerContent))
                {
                    content = content.Substring(opts.headerContent.Length);
                    Console.WriteLine("Remove header to file '" + dfile + "'");
                }
                else
                {
                    Console.WriteLine("WARNING: cannot remove header from file '" + sfile + "', all content is copied");
                }
            }

            File.WriteAllText(dfile, content);
        }

        static void Main(string[] args)
        {
            Options opts = new Options();

            if (!opts.Parse(args))
                return;

            if (!opts.Validate())
                return;

            RunDir(opts.inputDir, opts.outputDir, opts);
        }
    }
}
