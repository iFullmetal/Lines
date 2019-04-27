using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace _5inArow
{
    class Progressbar
    {
        static public void draw(int size) //отрисовка прогресбара
        {
            for (int i = 0; i < size; i++)
            {
                Program.drawCube(i, 21, ConsoleColor.Gray);
                Thread.Sleep(50);
                Console.SetCursorPosition(0, 23);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine(((i / (double)size) * 100.0).ToString() + "%");//вывожу количество процентов заполнения прогресбара
            }
            Console.SetCursorPosition(0, 23);
            Console.WriteLine(100.0 + "%"); //100 в конце не успевает вывестись, по этому делаю это здесь
        }

        static public void clear(int size)//очистка от заполненного прогресбара
        {
            for (int i = 0; i < size; i++)
            {
                Program.drawCube(i, 21, ConsoleColor.Black);
            }
            Console.SetCursorPosition(0, 23);
            Console.WriteLine("     ");
        }
    }
}
