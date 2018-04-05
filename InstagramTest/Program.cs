using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InstagramTest
{
    class Program
    {
        private string username;
        private Web web;        
        private int imgCountForView = 20;

        static void Main(string[] args)
        {
            var program = new Program();
            program.EnterUsername();
            program.web = new Web();
            List<Edge> edges = program.web.GetLinksPhotos(program.username);

            if (edges != null)
            {                
                for (int i = 0, imagesCount = 1; i < edges.Count && imagesCount <= program.imgCountForView; i++)
                {
                    var node = edges[i].node;
                    if (node.__typename == "GraphImage")
                    {
                        Logger.Success("{0} - {1}", imagesCount, node.display_url + "\n");
                        imagesCount++;
                    }
                }
            }
            Console.ReadKey();
        }

        private void EnterUsername()
        {
            Logger.Info("Введите юзернейм в Instagram:");
            bool result = false;
            while (!result)
            {
                username = Console.ReadLine();
                Regex regex = new Regex(@"[^\w\d_.]+");
                if (regex.IsMatch(username))
                {
                    username = default;
                    Logger.Info("В юзернеймах можно использовать только буквы, цифры, символы подчеркивания и точки, введите еще раз:");
                }
                else if (String.IsNullOrEmpty(username))
                {
                    username = default;
                    Logger.Info("Юзернейм не может быть пустым, введите еще раз:");
                }
                else
                {
                    result = true;
                }
            }
        }
    }
}