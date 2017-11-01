using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotCop
{
    class Program
    {
        static void Main(string[] args)
        {
            ImageDetector detector = new ImageDetector();
            detector.LoadTemplate("template.gif");
            Console.WriteLine("Getting PNGs:");
            var suspects = Directory.EnumerateFiles(@"..\", "*.png", SearchOption.AllDirectories).ToArray();
            var results = detector.DetectAndOutMatch(suspects);
            File.WriteAllLines("result.txt", results);
        }
    }

    public class ImageDetector
    {
        private Image<Bgra, byte> _template; // Image A
        private Dictionary<string, Image<Bgra, byte>> _suspects;

        public ImageDetector()
        {

        }
        public void LoadTemplate(string path)
        {
            _template = new Image<Bgra, byte>(path); // Image A

        }
        
        public IEnumerable<string> DetectAndOutMatch(string[] sourceNames)
        {
            List<Task<string>> tasks = new List<Task<string>>();
            for(int i = 0;  i < sourceNames.Count();i++)
            {
                var s = sourceNames[i];
                var p = i;
                var task = new Task<string>(() => 
                {
                    using (var source = new Image<Bgra, byte>(s))
                    {
                        var result = DetectOne(source, _template);
                        if (result == null)
                        {
                            return "[Error]"+ s;
                        }
                        if (result !=null && result.Value)
                        {

                            Console.WriteLine("{0} => {1}", s, "Yes");
                            return s;
                        }
                        else
                        {
                            if (p % 100 == 0)
                            {
                                Console.WriteLine("{0} / {1}", p, sourceNames.Count());
                            }
                        }
                    }
                    return null;
                });
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());
            return tasks.Where(t=>t.Result!=null).Select(t=>t.Result).ToList();
        }
        private bool? DetectOne(Image<Bgra, byte> source, Image<Bgra, byte> template)
        {
            try
            {
                using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    if (maxValues[0] > 0.9)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch 
            {
                return null;
            }
            
        }
    }
}
