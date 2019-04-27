using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace _5inArow
{
    struct Vec2i //структура для хранения позиций
    {
        public int x, y;
        public Vec2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public static bool operator !=(Vec2i first, Vec2i second)
        {
            return !(first.x == second.x && first.y == second.y);
        }
        public static bool operator==(Vec2i first, Vec2i second)
        {
            return first.x == second.x && first.y == second.y;
        }
    }
    class Game
    {
        int sizeX = 10;
        int sizeY = 10;
        Vec2i cursor = new Vec2i(0, 0);//текущая позиция курсора
        Vec2i chosenCube = new Vec2i(-1, -1); //кубик, выбранный курсором для перемещения
        Vec2i destenation = new Vec2i(-1, -1); //позиция, в которую выбранный куб должен быть перемещен
        bool empty = true;
        int colorQuantity = 2;//ставлю два цвета для кубиков, для того чтобы проще было продемонстрировать удаление 
        Cube[,] matrix;
        PathFinder pathFinder;
        public Game(int sizeX, int sizeY)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            matrix = new Cube[sizeY, sizeX];
            pathFinder = new PathFinder(this);

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    matrix[i, j] = new Cube(ConsoleColor.Black); //заполняю матрицу
                }
            }
        }
        public void addCubes()
        {
            Random r = new Random();
            int clr = r.Next(1, 1 + colorQuantity);
            //устанавливаю по 3 кубика в случайные свободные позиции матрицы
            for (int i = 0; i < 8; i++)
            {
                int x = 0, y = 0;
                do
                {
                    x = r.Next(0, sizeX);
                    y = r.Next(0, sizeY);
                }
                while (matrix[y, x].isFilled == true);//рандомлю координаты до тех пора, пока по таким позициям в массиве не будет пусто
                Program.drawCube(x * 3, y * 2, (ConsoleColor)(clr)); //рисую новый кубик
                matrix[y, x].isFilled = true; //заполняю его позицию в матрице, терперь сюда уже нельзя поставить другой кубик
                matrix[y, x].clr = (ConsoleColor)(clr); //заполняю его цвет
                Thread.Sleep(10);
            }
        }
        public bool isFull() //проверка на то, полна ли матрица
        {
            bool full = true;
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (matrix[i, j].isFilled == false)//если хотя бы один элемент пуст, то и матрица неполная
                    {
                        full = false;

                    }
                    else
                    {
                        empty = false;
                    }
                }
            }
            return full;
        }

        void cleanIt(List<List<PositionForClearing>> brokenCubes) //удаление линий(включая пересекающиеся)
        {

            for (int i = 0; i < brokenCubes.Count(); i++) //проход по списку линий
            {
                for (int j = 0; j < brokenCubes[i].Count(); j++) //проход по элементам линий
                {
                    //освобождаю их и чищу место на экране
                    matrix[brokenCubes[i][j].y, brokenCubes[i][j].x].isFilled = false;
                    matrix[brokenCubes[i][j].y, brokenCubes[i][j].x].clr = 0;
                    Program.drawCube(brokenCubes[i][j].x * 3, brokenCubes[i][j].y * 2, ConsoleColor.Black);
                }
            }
        }
        bool sameLineFinder(ref List<PositionForClearing> line) //проверяет, можно ли удалить что-то в этой линии(от 5 кубиков одного цвета и более), если да то line будет хранить эти кубики, а функция вернет правду
        {
            int begin = 0;//начало линии повторов
            int end = 0;//конец линии повторов
            int matched = 0; //количество повторов
            //поиск линий повторений
            for (int i = 1; i < line.Count(); i++)
            {
                if (matrix[line[i].y, line[i].x].isFilled && matrix[line[i - 1].y, line[i - 1].x].isFilled && matrix[line[i].y, line[i].x].clr == matrix[line[i - 1].y, line[i - 1].x].clr) //если прошлый и текущий одного цвета
                {
                    end = i; //то позиция стает концом линии для удаления
                    matched++;
                }
                else if (matched >= 4) { break; }//если совпадения прервались, но их >= пяти(4 потому что первая позиция уже заранее существует из-за проверки в верхнем ифе), то останавливаю поиск
                else if (matched < 4) //если совпадения прервались, но и[ было меньше 5, то начинаю с новой позиции
                {
                    matched = 0;
                    begin = i;
                }
            }
            if (matched >= 4) //если совпадений больше пяти
            {
                List<PositionForClearing> forBreaking = new List<PositionForClearing>();
                for (int i = begin; i <= end; i++)  //то копирую повторяющуюся часть линии
                {
                    forBreaking.Add(line[i]);
                }
                line = forBreaking; //список из аргументов, отправленный по ссылке теперь хранит кубики, которые необходимо удалить
                return true;
            }
            return false;

        }
        public void breakLine()
        {

            //эти два списка нужны для задания E. Вместо того, чтобы удалять линию сразу, я сохраняю ее тут и в функции cleanIt удаляю уже все вместе
            List<PositionForClearing> cubeLines = new List<PositionForClearing>();
            List<List<PositionForClearing>> forBreaking = new List<List<PositionForClearing>>();


            for (int startPos = 0; startPos <= 5; startPos++)//начальная координата для поиска(начинаю с 1 - 9, потом 2 - 9, ... 5 - 9) с 1 потому что проверка на цвет будет идти с прошлым элементом, т.е. с нулем
            {

                for (int line = 0; line < (sizeX < sizeY ? sizeX : sizeY); line++) //текущая линия(беру меньшую из координат на случай, если матрица не квадратная)
                {
                    //для вертикали
                    for (int i = startPos; i < sizeY; i++)
                    {
                        cubeLines.Add(new PositionForClearing(line, i, matrix[i, line].clr));
                    }
                    if (sameLineFinder(ref cubeLines)) //если в линии есть 5 или более повторений, то т.к. список передается по ссылке в функция sameLineFinder уже кладет в него вместо оригинальной линии те самые повторяющиеся кубики
                    {
                        forBreaking.Add(new List<PositionForClearing>(cubeLines)); //то добавляю линию в список линиий для удаления
                    }
                    cubeLines.Clear();

                    //для горизонтали

                    for (int i = startPos; i < sizeX; i++)
                    {
                        cubeLines.Add(new PositionForClearing(i, line, matrix[line, i].clr));
                    }
                    if (sameLineFinder(ref cubeLines)) //если в линии есть 5 или более повторений, то т.к. список передается по ссылке в функция sameLineFinder уже кладет в него вместо оригинальной линии те самые повторяющиеся кубики
                    {
                        forBreaking.Add(new List<PositionForClearing>(cubeLines)); //то добавляю линию в список линиий для удаления
                    }
                    cubeLines.Clear();

                    //диагонали
                    int minLength = (sizeX < sizeY ? sizeX : sizeY); //на случай, если поле не квадратное

                    for (int i = startPos; i < minLength; i++)
                    {
                        //главная диагональ
                        for (int j = 0, k = i; j < minLength - i && k < minLength; j++, k++)
                        {
                            cubeLines.Add(new PositionForClearing(j, k, matrix[k, j].clr)); //добавляю элемент диагонали в список
                        }
                        if (sameLineFinder(ref cubeLines)) //если в линии есть 5 или более повторений, то т.к. список передается по ссылке в функция sameLineFinder уже кладет в него вместо оригинальной линии те самые повторяющиеся кубики
                        {
                            forBreaking.Add(new List<PositionForClearing>(cubeLines)); //то добавляю линию в список линиий для удаления
                        }
                        cubeLines.Clear(); //чищу линию для следующих действий, т.к. использовал ее по ссылке

                        for (int j = 0, k = i; j < minLength - i && k < minLength; j++, k++)
                        {
                            cubeLines.Add(new PositionForClearing(k, j, matrix[j, k].clr));
                        }
                        if (sameLineFinder(ref cubeLines))
                        {
                            forBreaking.Add(new List<PositionForClearing>(cubeLines));
                        }
                        cubeLines.Clear();

                        //побочная диагональ

                        for (int j = minLength - 1, k = i; j >= startPos && k < minLength; j--, k++)
                        {
                            cubeLines.Add(new PositionForClearing(j, k, matrix[k, j].clr));
                        }
                        if (sameLineFinder(ref cubeLines))
                        {
                            forBreaking.Add(new List<PositionForClearing>(cubeLines));
                        }
                        cubeLines.Clear();
                        for (int k = i, j = startPos; k >= startPos && j < minLength; k--, j++)
                        {
                            cubeLines.Add(new PositionForClearing(k, j, matrix[j, k].clr));
                        }
                        if (sameLineFinder(ref cubeLines))
                        {
                            forBreaking.Add(new List<PositionForClearing>(cubeLines));
                        }
                        cubeLines.Clear();
                    }

                   
                }
            }
            cleanIt(forBreaking); //вызываю функцию удаления, отправляя туда список линий, которые необходимо удалить
        }
        void moveCursor(int dirX, int dirY)//смена позиции курсора
        {
            if (cursor.x + dirX >= 0 && cursor.x + dirX < sizeX && cursor.y + dirY >= 0 && cursor.y + dirY < sizeY)//если позиция не выходит за пределы игровог поля, то эта позция стает текущей
            {
                cursor.x += dirX;
                cursor.y += dirY;
            }
        }
        public void InputHandler()
        {
            if (empty) return; //если не было поставлено ни одного кубика, то смысла пользоваться курсором нет

            ConsoleKeyInfo key = new ConsoleKeyInfo();
            bool cubesChoosed = false;
            do
            {
                key = Console.ReadKey();
                if (key != null) clearCursor();//если была нажата хоть какая-нибудь клавиша, то очищаю прошлую позицию курсора

                switch (key.Key)//двигаю курсов в завсисмости от направления нажатой стрелочки
                {
                    case ConsoleKey.DownArrow:
                        moveCursor(0, 1);
                        break;
                    case ConsoleKey.UpArrow:
                        moveCursor(0, -1);
                        break;
                    case ConsoleKey.LeftArrow:
                        moveCursor(-1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        moveCursor(1, 0);
                        break;
                    case ConsoleKey.Spacebar: //при нажатии на пробел выбирается куб
                        if(matrix[cursor.y, cursor.x].isFilled != false && chosenCube == new Vec2i(-1, -1)) //если до этого кубик, который нужно переместить не выбран
                        {
                            chosenCube = cursor; //то выбирается он
                        }
                        else if (matrix[cursor.y, cursor.x].isFilled == false && chosenCube != new Vec2i(-1, -1)) //если он уже выбран, то выбирается место, куда его необходимо переместить
                        {
                            destenation = cursor;
                            cubesChoosed = true; //кубики выбраны, цикл перестает работать
                        }
                        break;
                }
                drawCursor();//отрисовываю курсор в новой позиции

                Console.BackgroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(50, 10);
                

            } while (!cubesChoosed); //считывать ввод буду до тех пор, пока координаты для перемещения не будут выбраны

            pathFinder.clear();//инициализирую поля для поиска пути

            if (pathFinder.findPath()) //ищу путь
            {
                pathFinder.drawPath(); //если он найден, то рисую его
                matrix[destenation.y, destenation.x] = matrix[chosenCube.y, chosenCube.x];
                matrix[chosenCube.y, chosenCube.x] = new Cube(ConsoleColor.Black);

                Program.drawCube(destenation.x, destenation.y, matrix[destenation.y, destenation.x].clr);
                Program.drawCube(chosenCube.x, chosenCube.y, ConsoleColor.Black);
            }

            chosenCube = new Vec2i(-1, -1); 
            destenation = new Vec2i(-1, -1);


        }
        void clearCursor()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.SetCursorPosition(cursor.x * 3 + j, cursor.y * 2 + i);
                    Console.BackgroundColor = matrix[cursor.y, cursor.x].clr;
                    Console.Write(" ");
                }
            }
        }
        void drawCursor()
        {

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.SetCursorPosition(cursor.x * 3 + j, cursor.y * 2 + i);
                    Console.BackgroundColor = matrix[cursor.y, cursor.x].clr;
                    Console.Write("#");
                }
            }
        }

        class PathFinder
        {
            Game game;
            int[,] way;
            int step;
            public PathFinder(Game game)
            {
                this.game = game;
                clear();
            }
            public void clear() //начальная инициализация полей, необходимых для поиска пути
            {
                way = new int[game.sizeY, game.sizeX];
                step = 1;
                for (int i = 0; i < game.sizeY; i++)
                {
                    for (int j = 0; j < game.sizeX; j++)
                    {
                        way[i, j] = 0;
                    }
                }
            }
            bool isValid(int x, int y)
            {
                return x > -1 && x < game.sizeX && y > -1 && y < game.sizeY;
            }
            bool move(int x, int y) //поиск возможных путей. если путь найден, то заполняю его номером следующего шага
            {
                bool passed = false;
                if(isValid(x + 1, y) && game.matrix[y, x + 1].isFilled == false && way[y, x + 1] == 0)
                {
                    way[y, x + 1] = step + 1;
                    passed = true;
                }
                if (isValid(x - 1, y) && game.matrix[y, x - 1].isFilled == false && way[y, x - 1] == 0)
                {
                    way[y, x - 1] = step + 1;
                    passed = true;
                }
                if (isValid(x, y + 1) && game.matrix[y + 1, x].isFilled == false && way[y + 1, x] == 0)
                {
                    way[y + 1, x] = step + 1;
                    passed = true;
                }
                if (isValid(x, y - 1) && game.matrix[y - 1, x].isFilled == false && way[y - 1, x] == 0)
                {
                    way[y - 1, x] = step + 1;
                    passed = true;
                }
                return passed;
            }
            public bool findPath() //поиск пути по волновому алгоритму(я написал заново немного отлично от того, что написали вы)
            {
                if(game.chosenCube.x < 0 || game.chosenCube.x > game.sizeX || game.chosenCube.y < 0 || game.chosenCube.y > game.sizeY)
                {
                    return false;
                }
                way[game.chosenCube.y, game.chosenCube.x] = step;
                bool deadEnd;
                do
                {
                    deadEnd = true;
                    //делаю ход на шаг вперед везде, где это возможно
                    for(int i = 0; i < game.sizeY; i++)
                    {
                        for(int j = 0; j < game.sizeX; j++)
                        {
                            if(way[i, j] == step)
                            {
                                if(move(j, i))
                                {
                                    deadEnd = false;//если хотябы один шаг удачен, то алгоритм не в тупике
                                }
                            }
                        }
                    }
                    step++;
                    if (way[game.destenation.y, game.destenation.x] == step) return true;//если в конечной координате лежит шаг, то цель достигнута

                } while (!deadEnd); //пока поиск не зашел в тупик
                return false;
            }
            public void drawPath()
            {
                Vec2i curpos = game.destenation;

                List<Vec2i> succsessfulWay = new List<Vec2i>();
                succsessfulWay.Add(curpos);
                do
                {
                    if (isValid(curpos.x + 1, curpos.y) && way[curpos.y, curpos.x + 1] == step - 1)
                    {
                        curpos.x++;
                    }
                    if (isValid(curpos.x - 1, curpos.y) && way[curpos.y, curpos.x - 1] == step - 1)
                    {
                        curpos.x--;
                    }
                    if (isValid(curpos.x, curpos.y + 1) && way[curpos.y + 1, curpos.x] == step - 1)
                    {
                        curpos.y++;
                    }
                    if (isValid(curpos.x, curpos.y - 1) && way[curpos.y - 1, curpos.x] == step - 1)
                    {
                        curpos.y--;
                    }
                    succsessfulWay.Add(curpos);
                    step--;
                } while (curpos != game.chosenCube);

                succsessfulWay.Reverse();// так как получение пути шло с финиша, а вывести я хочу красиво от начала до конца, то необходимо его реверсировать
                foreach (Vec2i pos in succsessfulWay)//вывод пути от начала до конца
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Console.SetCursorPosition(pos.x * 3 + j, pos.y * 2 + i);
                            Console.BackgroundColor = game.matrix[pos.y, pos.x].clr;
                            Console.Write("#");
                        }
                    }
                    Thread.Sleep(50);
                }
                Thread.Sleep(700);
                foreach (Vec2i pos in succsessfulWay) //очистка экрана от пути
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Console.SetCursorPosition(pos.x * 3 + j, pos.y * 2 + i);
                            Console.BackgroundColor = game.matrix[pos.y, pos.x].clr;
                            Console.Write(" ");
                        }
                    }
                }
            }
        }
    }
}
