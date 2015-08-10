using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ProjectTaskRunner
{
    class TaskParser
    {
        public static Dictionary<string, string[]> LoadTasks(string configPath)
        {
            Dictionary<string, string[]> dic = new Dictionary<string, string[]>();

            try
            {
                string document = File.ReadAllText(configPath);
                JObject root = JObject.Parse(document);

                var children = root["scripts"].Children<JProperty>();

                foreach (var child in children)
                {
                    if (child.Value.Type == JTokenType.Array)
                        dic.Add(child.Name, child.Value.Values().Select(v => v.ToString()).ToArray());
                    else if (child.Value.Type == JTokenType.String)
                        dic.Add(child.Name, new[] { child.Value.ToString().Trim('"') });
                }
            }
            catch
            {
                return null;
                // TODO: Implement logger
            }

            return dic;
        }
    }
}
