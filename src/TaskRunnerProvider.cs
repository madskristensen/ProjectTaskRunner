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
    [TaskRunnerExport(Constants.FILENAME)]
    class TaskRunnerProvider : ITaskRunner
    {
        private ImageSource _icon;

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

                if (!hierarchy.Children.Any() && !hierarchy.Children.First().Children.Any())
                    return null;

                return new TaskRunnerConfig(context, hierarchy, _icon);
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
                TaskRunnerNode task = new TaskRunnerNode(key, true)
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
    }
}
