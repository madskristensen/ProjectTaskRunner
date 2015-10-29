using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using ProjectTaskRunner.Helpers;

namespace ProjectTaskRunner
{
    internal class TrimmingStringComparer : IEqualityComparer<string>
    {
        private char _toTrim;
        private IEqualityComparer<string> _basisComparison;

        public TrimmingStringComparer(char toTrim)
            : this(toTrim, StringComparer.Ordinal)
        {
        }

        public TrimmingStringComparer(char toTrim, IEqualityComparer<string> basisComparer)
        {
            _toTrim = toTrim;
            _basisComparison = basisComparer;
        }

        public bool Equals(string x, string y)
        {
            string realX = x?.TrimEnd(_toTrim);
            string realY = y?.TrimEnd(_toTrim);
            return _basisComparison.Equals(realX, realY);
        }

        public int GetHashCode(string obj)
        {
            string realObj = obj?.TrimEnd(_toTrim);
            return realObj != null ? _basisComparison.GetHashCode(realObj) : 0;
        }
    }

    [TaskRunnerExport(Constants.FILENAME)]
    class TaskRunnerProvider : ITaskRunner
    {
        private ImageSource _icon;
        private HashSet<string> _dynamicNames = new HashSet<string>(new TrimmingStringComparer('\u200B'));

        public void SetDynamicTaskName(string dynamicName)
        {
            _dynamicNames.Remove(dynamicName);
            _dynamicNames.Add(dynamicName);
        }

        public string GetDynamicName(string name)
        {
            IEqualityComparer<string> comparer = new TrimmingStringComparer('\u200B');
            return _dynamicNames.FirstOrDefault(x => comparer.Equals(name, x));
        }

        public TaskRunnerProvider()
        {
            _icon = new BitmapImage(new Uri(@"pack://application:,,,/ProjectTaskRunner;component/Resources/project.png"));
        }

        public List<ITaskRunnerOption> Options
        {
            get { return null; }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

                if (!hierarchy.Children.Any())// && !hierarchy.Children.First().Children.Any())
                    return null;

                Telemetry.TrackEvent("Tasks loaded");

                return new TaskRunnerConfig(this, context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            ITaskRunnerNode root = new TaskRunnerNode(Constants.TASK_CATEGORY);

            string workingDirectory = Path.GetDirectoryName(configPath);

            Dictionary<string, string[]> scripts = TaskParser.LoadTasks(configPath);

            if (scripts == null)
                return root;

            TaskRunnerNode tasks = new TaskRunnerNode("Scripts");
            tasks.Description = "Scripts specified in the \"scripts\" JSON element.";
            root.Children.Add(tasks);

            foreach (var key in scripts.Keys.OrderBy(k => k))
            {
                // Add zero width space
                string commandName = GenerateCommandName(key);// key + "\u200B";
                SetDynamicTaskName(commandName);

                TaskRunnerNode task = new TaskRunnerNode(commandName, true)
                {
                    Command = new TaskRunnerCommand(workingDirectory, "cmd.exe", "/c " + string.Join(" && ", scripts[key])),
                    Description = string.Join(", ", scripts[key]),
                };

                foreach (string child in scripts[key])
                {
                    TaskRunnerNode childTask = new TaskRunnerNode(child, true)
                    {
                        Command = new TaskRunnerCommand(workingDirectory, "cmd.exe", "/c " + child),
                    };

                    task.Children.Add(childTask);
                }

                tasks.Children.Add(task);
            }

            return root;
        }

        private string GenerateCommandName(string commandName)
        {
            Random rnd = new Random(DateTime.Now.Millisecond + DateTime.Now.Second);
            int count = rnd.Next(99);

            return commandName + new string('\u200B', count);
        }
    }
}
