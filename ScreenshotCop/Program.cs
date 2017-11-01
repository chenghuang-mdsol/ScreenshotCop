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
            detector.LoadSuspects(suspects);
            var results = detector.DetectAndOutMatch();
            File.WriteAllLines("result.txt", results);
            //foreach (var r in results) { Console.WriteLine(r); }
            //Console.ReadKey();
        }
    }

    public class ImageDetector
    {
        private Image<Bgr, byte> _template; // Image A
        private Dictionary<string, Image<Bgr, byte>> _suspects;

        public ImageDetector()
        {

        }
        public void LoadTemplate(string path)
        {
            _template = new Image<Bgr, byte>(path); // Image A

        }
        public void LoadSuspects(string[] paths)
        {
            _suspects = paths.ToDictionary(o=>o, o => new Image<Bgr, byte>(o));
        }
        public IEnumerable<string> DetectAndOutMatch()
        {
            foreach(var s in _suspects)
            {
                var result = DetectOne( s.Value, _template);
                if (result)
                {
                    Console.WriteLine("{0} => {1}", s.Key, "Yes");
                    yield return s.Key;
                }
                else
                {
                    Console.WriteLine("{0} => {1}", s.Key, "No");
                }
            }
        }
        private bool DetectOne(Image<Bgr, byte> source, Image<Bgr, byte> template)
        {
            
            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                if (maxValues[0] > 0.9)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    //Rectangle match = new Rectangle(maxLocations[0], template.Size);
                    //imageToShow.Draw(match, new Bgr(Color.Red), 3);
                    return true;
                }
                return false;
            }
        }
    }
}
