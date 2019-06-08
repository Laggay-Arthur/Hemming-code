using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;


namespace Lab5
{//*****************************************Выполнил Лаггай Артур Владимирович********************************************************
    public partial class MainWindow : Window
    {
        int k, ///Кол-во информационных разрядов
            p; ///Кол-во проверочных разрядов

        char[,] H;///Проверочная матрица

        List<int> bt = new List<int>() { 1 };//Положение проверочных разрядов
        List<string> bits = new List<string>();//Положение проверочных битов
        List<List<int>> sindrom = new List<List<int>>();//Синдром ошибки
        List<char> Bi = new List<char>();//Значение проверочных битов


        public MainWindow() { InitializeComponent(); }
        /// <summary> Очистка всех полей </summary>
        /// <param name="sender" name="e"></param>
        void Clear_Click(object sender, RoutedEventArgs e)
        {
            kInput.Text = "39";

            TrueMSG1.Content = TrueMSG3.Content = TrueMSG.Text = TrueMSG2.Text = msgHemm.Text = msgHemm1.Text = sindromForERROR.Text = sindromFor2ERROR.Text = errorMSG.Text = msgERROR.Text = msg2ERROR.Text = errorMSG1.Text = errorMSG2.Text = msg.Text = msg1.Text = "";
            H = null;
            list1.Items.Clear();
            list2.Items.Clear();

            grid2.RowDefinitions.Clear();
            grid2.ColumnDefinitions.Clear();
            grid2.Children.Clear();
        }
        void Culculating_Click(object sender, RoutedEventArgs e)
        {
            k = p = 0;
            if (!int.TryParse(kInput.Text, out k) || k <= 0)
            { MessageBox.Show("Ошибка в входных данных!"); return; }
            bt.Clear();
            bits.Clear();
            sindrom.Clear();
            Bi.Clear();

            while (Math.Pow(2, p) < k + p + 1) p++;


            string message = "";
            Random rand = new Random();
            while (message.Length < k)//Генерируем информационные разряды
                message += rand.Next(0, 2) + "";//50 на 50
            msg.Text = message;//Выводим на форму
            bt.Add(1);
            for (int a = 1; a < p; a++)
            {
                bt.Add((int)Math.Pow(2, a));
                message = message.Insert(bt[a - 1] - 1, "0");
            }
            message = message.Insert(bt[bt.Count - 1] - 1, "0");


            getBits();
            bits.RemoveAt(0);//Удаляем пустую строку

            int index = 0;
            for (index = p - 1; index >= 0; index--)
            {
                List<int> buff = new List<int>();
                int pos = 1;
                for (int j = 0; j < k + p; j++)
                {
                    var item = bits[j];
                    if (item[index] == '1')
                        buff.Add(pos);

                    pos++;
                }
                sindrom.Add(buff);
            }

            bool res = false;
            for (int i = 0; i < p; i++)
            {
                for (int j = 0; j < sindrom[i].Count; j++)
                    if (message[sindrom[i][j] - 1] == '1') res = !res;///По модулю два

                Bi.Add(res ? '1' : '0');
                res = false;
            }

            for (int i = 0; i < p; i++)//Строим код Хемминга
                message = message.Remove(bt[i] - 1, 1).Insert(bt[i] - 1, Bi[i] + "");
            msgHemm.Text = message;//Выводим код Хемминга на форму
            int ERRORindex = index = rand.Next(0, k + p); ///Положение ошибки
            string ERRORmessage = msgERROR.Text = message.Remove(index, 1).Insert(index, message[index] == '0' ? "1" : "0"); //Добавили однокатную ошибку


            List<char> SindromForError = new List<char>();
            sindromForERROR.Text = "";
            for (int i = 0; i < p; i++)
            {//Считаем контрольные суммы для сообщения с однократной ошибкой
                res = false;
                for (int j = 0; j < sindrom[i].Count; j++)
                    if (ERRORmessage[sindrom[i][j] - 1] == '1') res = !res;///По модулю два

                SindromForError.Add(res ? '1' : '0');
            }
            for (int i = p - 1; i >= 0; i--)
                sindromForERROR.Text += SindromForError[i];


            bits.Clear();
            getBits();
            bits.RemoveAt(0);//Удаляем пустую строку
            if (H == null)
            {//Нет смысла генерировать таблицу каждый раз
                H = new char[p + 1, k + p + 1];//Строим проверочную матрицу для двукратных ошибок
                for (int j = 0; j < k + p; j++) for (int i = 0; i < p; i++)
                        H[i, j] = bits[j][i];//Заполняем проверочную матрицу
                for (int i = 0; i < k + p + 1; i++)
                    H[p, i] = '1';//Заполняем горизонталь
                for (int i = 0; i < p; i++)
                    H[i, k + p] = '0';//Заполняем вертикаль


                CreateGrid(p + 2, k + p + 1, grid2);//Создаём проверочную матрицу
                for (int i = 1; i < p + 2; i++) for (int j = 0; j < k + p + 1; j++)
                    {//Заполняем проверочную матрицу
                        (grid2.Children[j + (i * (k + p + 1))] as Label).Content = H[i - 1, j];
                    }
            }

            errorMSG.Text = "Ошибка в " + getSindromInDec(ref SindromForError) + "-м бите";
            int indx = 1, c = 1;
            string bt1 = "";
            list1.Items.Clear();
            for (int i = 0; i < sindrom.Count - 1; i++)
            {//Выводим синдром однократной ошибки
                var item = sindrom[i];
                bt1 = "S" + indx++ + "=" + "b" + c;
                for (int j = 0; j < item.Count; j++)
                    bt1 += "+" + ("a" + item[j]);
                c++;
                list1.Items.Add(bt1 + "=" + SindromForError[indx - 1]);
            }
            TrueMSG3.Content = "Код с исправленной ошибкой";
            TrueMSG.Text = msgHemm.Text;


            for (int i = 0; i < message.Length; i++)//Считаем синдром для двукратной ошибки
                if (message[i] == '1') res = !res;//По модулю два
            Bi.Add(res ? '1' : '0');

            msg1.Text = msg.Text;
            string msg2error = msgHemm1.Text = msgHemm.Text + Bi[Bi.Count - 1];//Добавили в конец проверочный бит
            msg2error = msg2error.Remove(ERRORindex, 1).Insert(ERRORindex, msg2error[ERRORindex] == '0' ? "1" : "0");//Добавили первую ошибку
            bool is2Error = false;//Создаём читы
            if (rand.Next(0, 2) == 1)//Добавлять или не добавлять, вот в чем вопрос...
            {
                rand = new Random();
                do index = rand.Next(0, k + p); //Положение второй ошибки
                while (ERRORindex == index);
                is2Error = true;//Читы активированы
                msg2error = msg2error.Remove(index, 1).Insert(index, msg2error[index] == '0' ? "1" : "0");//Добавили вторую ошибку
            }
            msg2ERROR.Text = msg2error;
            List<int> buff1 = new List<int>();

            for (int i = 0; i < msg2error.Length; i++)
                buff1.Add(i + 1);
            sindrom.Add(buff1);//Добавляем последний синдром
            List<char> SindromFor2Error = new List<char>();

            for (int i = 0; i < p + 1; i++)
            {//Считаем контрольные суммы для сообщения с двукратной ошибкой
                res = false;
                for (int j = 0; j < sindrom[i].Count; j++)
                    if (msg2error[sindrom[i][j] - 1] == '1') res = !res;//По модулю два

                SindromFor2Error.Add(res ? '1' : '0');
            }

            bt1 = "";
            indx = 0; c = 1;
            list2.Items.Clear();
            for (int i = 0; i < sindrom.Count - 1; i++)
            {//Выводим синдром ошибки
                var item = sindrom[i];
                bt1 = "S" + ++indx + "=" + "b" + c;
                for (int j = 0; j < item.Count; j++)
                    bt1 += "+" + ("a" + item[j]);
                c++;
                list2.Items.Add(bt1 + "=" + SindromFor2Error[indx - 1]);
            }

            bt1 = "S" + ++indx + "=";
            index = 1;
            int t = 1;
            for (int i = 1; i < msg2error.Length; i++)
            {//Выводим последний синдром
                res = false;
                for (int j = 0; j < bt.Count; j++)
                    if (i == bt[j]) { res = true; break; }

                bt1 += (res ? "b" + t++ : "a" + index++) + "+";
            }
            list2.Items.Add(bt1 + "b" + t + "=" + SindromFor2Error[indx - 1]);
            sindromFor2ERROR.Text = "";
            for (int i = 0; i < SindromFor2Error.Count; i++)
                sindromFor2ERROR.Text += SindromFor2Error[i];

            TrueMSG1.Content = TrueMSG2.Text = errorMSG1.Text = errorMSG2.Text = "";
            if (is2Error) errorMSG1.Text = "Сообщение содержит две ошибки!";//Используем читы
            else
            {
                SindromFor2Error.RemoveAt(SindromFor2Error.Count - 1);
                TrueMSG1.Content = "Код с исправленной ошибкой";
                TrueMSG2.Text = msgHemm.Text + Bi[Bi.Count - 1];
                errorMSG1.Text = "Ошибка в " + getSindromInDec(ref SindromFor2Error) + "-м бите";
                errorMSG2.Text = "Вторая ошибка не обнаружена";
            }
        }
        /// <summary> Переводит синдром ошибки в десятичную запись </summary>
        /// <param name="sindrom"></param>
        int getSindromInDec(ref List<char> sindrom)
        {
            int sum = 0;
            for (int i = 0; i < sindrom.Count; i++)
                if (sindrom[i] == '1') sum += bt[i];
            return sum;
        }
        /// <summary>Создаёт матрицу размером MxN в контейнере grid</summary>
        /// <param name="M" name="N" name="grid"></param>
        void CreateGrid(int M, int N, Grid grid)
        {
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.Children.Clear();

            for (int i = 0; i < M; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 20 });
                for (int j = 0; j < k; j++)
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 25 });
            }

            while (grid.ColumnDefinitions.Count > N)///Удаляем ненужные колонки
                grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.IndexOf(grid.ColumnDefinitions.Last()));

            for (int i = 0; i < M; i++) for (int j = 0; j < N; j++)
                {
                    var l = new Label
                    {
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    if (i == 0 && j < N - 1)
                        l.Content = "a" + (j + 1);
                    else l.Content = "";

                    Grid.SetColumn(l, j);
                    Grid.SetRow(l, i);
                    grid.Children.Add(l);
                }
        }
        /// <summary> Полный перебор комбинаций проверочных битов </summary>
        /// <param name="p"></param>
        void getBits()
        {
            char[] a_s = { '0', '1' };
            for (int i = 0; i < Math.Pow(2, p); i++)
            {
                string s = "";
                int ii = i;
                for (int j = 0; j < p; j++)
                {
                    s = a_s[ii % 2] + s;
                    ii /= 2;
                }
                bits.Add(s);
            }
        }
        void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { DragMove(); }
    }
}