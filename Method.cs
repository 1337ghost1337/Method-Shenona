using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp133
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите строку для записи в файл:");
            string text = Console.ReadLine();
            string fn_dano = "dano.bin", fn_rez = "rez.bin";
            // запись в файл
            //для закрытия потока применяется конструкция using
            using (FileStream fstream = new FileStream(fn_dano, FileMode.Create))
            {
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes(text);
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);
                Console.WriteLine("Текст записан в файл");
            }            
            
            
            byte[] dano = null;
            string rez = "", tmp = "";
            int n_dano = 0;
            // чи є файл
            FileInfo fileInf = new FileInfo(fn_dano);
            if (fileInf.Exists)
            {
                n_dano = (int)fileInf.Length;
            }
            else
            {
                Console.WriteLine("No file {0}", fn_dano);
                Console.Write("Press any key to continue . . . ");
                Console.ReadKey(true);
                return;
            }

            // читання
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(fn_dano, FileMode.Open)))
                    dano = br.ReadBytes(n_dano);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //кодування
            // 1) порахувати кількість байтів
            List<Simvol> spisok = new List<Simvol>();
            int i, k, j;
            Simvol sim;
            for (i = 0; i < n_dano; i++)
            {
                j = PoshukByte(spisok, dano[i]);
                if (j == -1)
                {
                    sim = new Simvol(dano[i]);
                    spisok.Add(sim);
                }
                else
                {
                    spisok[j].Kolvo++;
                }
            }
            // 2) відсортувати за полем Kolvo
            spisok.Sort(new SimvolKolvoComparer());
            // 3) побудувати коди через рекурсивну функцію
            BuduvatKod(spisok, 0, spisok.Count - 1, "");
            Console.WriteLine("Знайдено коди:");
            foreach (Simvol sp in spisok)
            {
                Console.WriteLine(sp);
            }
            Console.WriteLine("Кінець виводу кодів.");
            // 4) сформувати кодований рядок вигляду
            //rez = "111000101010010101101011001010101011100010101000111";
            rez = "";
            for (i = 0; i < n_dano; i++)
            {
                j = PoshukByte(spisok, dano[i]);
                if (j == -1)
                {
                    Console.WriteLine("error byte {}", dano[i]);
                }
                else
                {
                    rez += spisok[j].Kod;
                }
            }
            // 5) записати у файл
            // 5.1) коригування довжини
            while (rez.Length % 8 > 0)
                rez += "0";
            k = rez.Length / 8;
            byte b1;
            // 5.2) запис
            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(fn_rez, FileMode.Create)))
                {
                    //bw.Write(dano.Length);
                    for (i = 0; i < k; i++)
                    {
                        tmp = rez.Substring(i * 8, 8);
                        b1 = ToByte(tmp);
                        bw.Write(b1);
                    }
                }
                Console.WriteLine("Запис виконано");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //тест
            string test = "";
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(fn_rez, FileMode.Open)))
                {
                    for (i = 0; i < k; i++)
                    {
                        b1 = br.ReadByte();
                        tmp = FromByte(b1);
                        test += tmp;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Тест1 - послідовність бітів разом з 0, дописаними в кінці");
            Console.WriteLine("записано  rez ={0}", rez);
            Console.WriteLine("прочитано test={0}", test);
            //розкодування
            byte[] nove = new byte[n_dano];
            int i0 = 0;
            bool znaydeno = false;
            int zsuv = 0;
            for (i = 0; i < n_dano; i++)
            {
                nove[i] = ByteFromKod(spisok, test, i0,
                        ref znaydeno, ref zsuv);
                if (!znaydeno)
                {
                    Console.WriteLine("помилка розкодування байту з індексом {0} - замінено 0 ", i);
                    break;
                }
                i0 += zsuv;
            }
            Console.WriteLine("Тест2 - текст файлу");
            Console.WriteLine("зразок      dano ={0}", System.Text.Encoding.Default.GetString(dano));
            Console.WriteLine("розкодовано nove ={0}", System.Text.Encoding.Default.GetString(nove));

            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }

        private static byte ToByte(string s1)
        {
            //поверта байт для заданого рядка з 8 символів 
            byte b1 = 0;
            for (int i = 0; i < 8; i++)
            {
                b1 *= 2;
                if (s1[i] == '1')
                    b1 += 1;
            }
            return b1;
        }

        private static string FromByte(byte b0)
        {
            //поверта рядок з 8 символів для заданого байту
            byte b1 = b0;
            int x;
            string s1 = "";
            for (int i = 0; i < 8; i++)
            {
                x = b1 % 2;
                if (x == 1)
                    s1 = '1' + s1;
                else
                    s1 = '0' + s1;
                b1 /= 2;
            }
            return s1;
        }

        private static int PoshukByte(List<Simvol> spisok, byte b1)
        {
            //поверта індекс першого знайденого елементу 
            //списку spisok, або -1, якщо такого нема
            int j = -1;
            for (int i = 0; i < spisok.Count; i++)
            {
                if (spisok[i].B == b1)
                {
                    j = i;
                    break;
                }
            }
            return j;
        }

        private static byte ByteFromKod(
            List<Simvol> spisok, string str, int i0,
            ref bool znaydeno, ref int zsuv)
        {
            //поверта byte першого знайденого елементу в spisok,
            //код якого рівний підрядку str, починаючи з позиції i0            
            //або 0, якщо такого нема
            //якщо є, znaydeno = true, zsuv - довжина його коду 
            znaydeno = false;
            zsuv = 0;
            int i, j, k, d;
            byte rez = 0;
            for (i = 0; i < spisok.Count; i++)
            {
                //порівняння підрядків
                d = spisok[i].Kod.Length;
                k = 0;
                for (j = 0; j < d; j++)
                {
                    if (spisok[i].Kod[j] == str[i0 + j])
                    {
                        k++;
                    }
                    else
                    {
                        break;
                    }
                }
                //якщо знайдено
                if (k == d)
                {
                    znaydeno = true;
                    rez = spisok[i].B;
                    zsuv = d;
                    break;
                }
            }
            return rez;
        }

        private static void BuduvatKod(List<Simvol> spisok, int i1, int i2, string kod)
        {
            //рекурсивна функція заповнює поле Kod 
            //в елементах списку spisok
            //з індексами з i1 по i2 включно
            //kod - початок коду, знайдений на попередніх етапах
            if (i1 == i2)
            {
                spisok[i1].Kod = kod;
                return;
            }
            int kolvo1 = spisok[i1].Kolvo;
            int kolvo2 = spisok[i2].Kolvo;
            int k = i1, j1 = i1, j2 = i2;
            //знайти j1, j2 - елементи на границі, 
            //що ділять інтервал з i1 по i2 на близькі 
            //за кількістю частини
            while (j2 - j1 > 1)
            {
                if (kolvo1 < kolvo2)
                {
                    j1++;
                    kolvo1 += spisok[j1].Kolvo;
                }
                else
                {
                    j2--;
                    kolvo2 += spisok[j2].Kolvo;
                }
            }
            //тут j2-j1==1 - границя між ними
            BuduvatKod(spisok, i1, j1, kod + "0");
            BuduvatKod(spisok, j2, i2, kod + "1");
        }
    }


    class Simvol
    {
        //клас описує 1 байт вхідного файлу
        public byte B { get; set; } //байт
        public int Kolvo { get; set; } //кількість
        public string Kod { get; set; } //код

        public Simvol(byte b1)
        {
            B = b1;
            Kolvo = 1;
            Kod = "";
        }

        public override string ToString()
        {
            //лише для тестування повертається 1 рядком
            //байт, кількість і код
            return
            " (int)byte=" + ((int)B).ToString() +
            " Kolvo=" + Kolvo.ToString() +
            " Kod=" + Kod;
        }
    }

    class SimvolKolvoComparer : IComparer<Simvol>
    {
        //клас порівняння за кількістю
        //в порядку спадання
        public int Compare(Simvol p1, Simvol p2)
        {
            if (p1.Kolvo > p2.Kolvo)
                return -1;
            else if (p1.Kolvo < p2.Kolvo)
                return 1;
            else
                return 0;
        }
    } 
}
    

