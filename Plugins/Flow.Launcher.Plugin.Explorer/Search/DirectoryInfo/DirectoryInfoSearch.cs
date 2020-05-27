﻿using Flow.Launcher.Infrastructure;
using Flow.Launcher.Plugin.SharedCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Flow.Launcher.Plugin.Explorer.Search.DirectoryInfo
{
    public class DirectoryInfoSearch
    {
        private PluginInitContext _context;

        private Settings _settings;

        public DirectoryInfoSearch(Settings settings, PluginInitContext context)
        {
            _context = context;
            _settings = settings;
        }

        internal List<Result> DirectoryAllFilesFoldersSearch(Query query, string search)
        {
            return DirectorySearch(SearchOption.AllDirectories, query, search);
        }

        internal List<Result> TopLevelDirectorySearch(Query query, string search)
        {
            return DirectorySearch(SearchOption.TopDirectoryOnly, query, search);
        }

        private List<Result> DirectorySearch(SearchOption searchOption, Query query, string search)
        {
            var results = new List<Result>();
            //var hasSpecial = search.IndexOfAny(_specialSearchChars) >= 0;
            string incompleteName = "";
            //if (hasSpecial || !Directory.Exists(search + "\\"))
            //// give the ability to search all folder when starting with >
            //if (incompleteName.StartsWith(">"))
            //{
            //    searchOption = SearchOption.AllDirectories;

            //    // match everything before and after search term using supported wildcard '*', ie. *searchterm*
            //    incompleteName = "*" + incompleteName.Substring(1);
            //}

            if (!search.EndsWith("\\"))
            {
                // not full path, get previous level directory string
                var indexOfSeparator = search.LastIndexOf('\\');

                incompleteName = search.Substring(indexOfSeparator + 1).ToLower();

                search = search.Substring(0, indexOfSeparator + 1);
            }

            incompleteName += "*";

            var folderList = new List<Result>();
            var fileList = new List<Result>();

            try
            {
                var directoryInfo = new System.IO.DirectoryInfo(search);
                var fileSystemInfos = directoryInfo.GetFileSystemInfos(incompleteName, searchOption);

                foreach (var fileSystemInfo in fileSystemInfos)
                {
                    if ((fileSystemInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;

                    if (fileSystemInfo is System.IO.DirectoryInfo)
                    {
                        folderList.Add(ResultManager.CreateFolderResult(fileSystemInfo.Name, fileSystemInfo.FullName, fileSystemInfo.FullName, query, true, false));
                    }
                    else
                    {
                        fileList.Add(ResultManager.CreateFileResult(fileSystemInfo.FullName, query, true, false));
                    }
                }
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException || e is ArgumentException)
                {
                    results.Add(new Result { Title = e.Message, Score = 501 });

                    return results;
                }

                throw;
            }

            // Intial ordering, this order can be updated later by UpdateResultView.MainViewModel based on history of user selection.
            return results.Concat(folderList.OrderBy(x => x.Title)).Concat(fileList.OrderBy(x => x.Title)).ToList(); //<============= MOVE OUT
        }
    }
}
