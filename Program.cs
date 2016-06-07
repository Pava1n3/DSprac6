using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    public class Program
    {
        public static Piet piet = new Piet();

        static void Main(string[] args)
        {
            //StreamReader sr = new StreamReader("numbers.txt");

            PrioQ pq = new PrioQ();
            Printer A = new Printer(), B = new Printer(), C = new Printer();
            int tijdstap = 1, regel = 0;

            //hahahaha

            //invoer naar prioq plaatsen
            string read = Console.ReadLine();
            string[] split;

            while(read != "sluit")
            {
                regel++;
                split = read.Split();
                
                Klant k = new Klant(Convert.ToInt32(split[0]), Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), regel);
                pq.Add(k);
                read = Console.ReadLine();
                if (regel == 80000)
                    read = "sluit";
            }

            pq.BuildHeap();
            piet.klantCount = pq.Count;

            //methode voor tijdstap
            //misschien klant dqen en aan rij toevoegen
            //printer die klaar is rolt de plaat naar piets Stack
            //klanten wiens printwerk klaar is stappen weg, klanten daarachter beginnen al met printen (volgende ronde 1 stap aan het printen)
            //als Piet klaar is met snijden legt hij het direct op de toonbank
            //als Piet niet bezig is met snijwerk pakt hij de bovenste bouwplaat (zelfde beurt nog beginnen)
            Klant huidig = new Klant(0, 0, 0, 0);
            if (pq.Count > 0)
            {
                huidig = pq.ExtractMin();
            }

            while(piet.klantCount > 0)
            {
                if (tijdstap == huidig.binnenkomst)
                {
                    //vind een rij
                    KortsteRij(A, B, C, huidig, tijdstap);
                    if(pq.Count > 0)    //als er nog klanten moeten komen, Dequeue de klant
                    {
                        huidig = pq.ExtractMin();  //nieuwe klant
                        
                        while(huidig.binnenkomst == tijdstap)
                        {
                            KortsteRij(A, B, C, huidig, tijdstap);

                            if (pq.Count > 0)
                                huidig = pq.ExtractMin();
                        }
                    }
                }

                //printers afhandelen   
                A.doePrinter(tijdstap);
                B.doePrinter(tijdstap);
                C.doePrinter(tijdstap);

                //piet afhandelen
                piet.DoePiet(tijdstap);

                tijdstap++;
            }

            int langstInDeRij, langstInDeRijTijd, langstNaPrint, langstNaPrintTijd, langstNaBinnen, LangstNaBinnenTijd;

            //Regelnummer van de klant die het langst in de rij heeft gestaan
            #region RijWachten
            if (A.langstGewacht >= B.langstGewacht)
                if (A.langstGewacht >= C.langstGewacht)
                {
                    langstInDeRij = A.lGRegelNummer;
                    langstInDeRijTijd = A.langstGewacht;
                }
                else
                {
                    langstInDeRij = C.lGRegelNummer;
                    langstInDeRijTijd = C.langstGewacht;
                }
            else if (B.langstGewacht >= C.langstGewacht)
            {
                langstInDeRij = B.lGRegelNummer;
                langstInDeRijTijd = B.langstGewacht;
            }
            else
            {
                langstInDeRij = C.lGRegelNummer;
                langstInDeRijTijd = C.langstGewacht;
            }
            #endregion

            //Regelnummer van de klant die het langst na het printen heeft gewacht
            langstNaPrint = piet.lGNPRegelNummer;
            langstNaPrintTijd = piet.langstGewachtNaPrinten;

            //Klant die het langst gewacht heeft na binnenkomst
            langstNaBinnen = piet.LGNBRegelNummer;
            LangstNaBinnenTijd = piet.langstGewachtNaBinnenkomst;

            //Write de resultaten
            Console.WriteLine(langstInDeRij + ": " + langstInDeRijTijd);
            Console.WriteLine(langstNaPrint + ": " + langstNaPrintTijd);
            Console.WriteLine(langstNaBinnen + ": " + LangstNaBinnenTijd);
            Console.WriteLine("sluitingstijd: " + tijdstap);

            //Console.ReadLine();
        }

        //Vind de korste rij en sluit de klant hier aan
        public static void KortsteRij(Printer a, Printer b, Printer c, Klant k, int tijdstap)
        {
            int ac = a.q.Count, bc = b.q.Count, cc = c.q.Count;
            Printer kortst;

            if (ac <= bc)
                if (ac <= cc)
                    kortst = a;
                else 
                    kortst = c;
            else if (bc <= cc)
                kortst = b;
            else
                kortst = c;

            kortst.q.Enqueue(k);

            if (kortst.pKlaar < tijdstap)
                kortst.NieuweKlant(tijdstap);
        }

    }

    //Implementatie van een PriorityQueue die Klant objecten bevat
    public class PrioQ
    {
        int n = 0;

        Klant[] heap = new Klant[100001];

        public PrioQ()
        { }

        public void Add(Klant k)
        {
            n++;
            heap[n] = k;
        }

        public void Insert(Klant k)
        {
            n++;
            heap[n] = k;
            Rootify(n);
        }

        public Klant ExtractMin()
        {
            Klant res = heap[1];
            heap[1] = heap[n];
            n--;
            Heapify(1);
            return res;
        }

        public void BuildHeap()
        {
            for(int i = n >> 1; i >= 1; i--)
            {
                Heapify(i);
            }
        }

        public void Heapify(int i)
        {
            int j = i;

            if (Left(i) <= n && heap[Left(i)] < heap[j])
                j = Left(i);
            if (Right(i) <= n && heap[Right(i)] < heap[j])
                j = Right(i);

            if(j != i)
            {
                Swap(i, j);
                Heapify(j);
            }
        }

        public void Rootify(int i)
        {
            if (i == 1 || heap[i] > heap[Parent(i)] || heap[i] == heap[Parent(i)])
                return;

            Swap(i, Parent(i));
            Rootify(Parent(i));
        }

        public void Swap(int i, int j)
        {
            Klant temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        public int Parent(int i)
        {
            return i >> 1;
        }

        public int Left(int i)
        {
            return i << 1;
        }

        public int Right(int i)
        {
            return (i << 1) | 1;
        }

        //Hiermee kan het aantal elementen in de queue opgevraagd worden
        public int Count
        {
            get { return n; }
        }
    }

    //Klant representeerd een klant en bevat de tijdstap van binnenkomst, de tijd die de klant bezig zal zijn met printen, snijtijd en het regelnummer
    public class Klant
    {
        public int binnenkomst, printTime, cutTime, lineNumber, printklaar;

        public Klant(int tijd, int printTijd, int snijTijd, int regelNummer)
        {
            binnenkomst = tijd;
            printTime = printTijd;
            cutTime = snijTijd;
            lineNumber = regelNummer;
        }

        public static bool operator <(Klant a, Klant b)
        {
            if (a.binnenkomst < b.binnenkomst)
                return true;
            else
                return false;
        }

        public static bool operator >(Klant a, Klant b)
        {
            return !(a < b);
        }
    }

    //Representeerd een Printer. De printer heeft een wachtrij en een methode om een tijdstap af te handelen.
    //Ook controleerd iedere printer wie er het langst in zijn rij staat (of haar rij, het is immers 2016 en printers mogen hun eigen geslacht kiezen)
    public class Printer
    {
        public int pKlaar = -1, langstGewacht = -1, lGRegelNummer;
        public Queue<Klant> q = new Queue<Klant>();

        public Printer()
        {}

        public void doePrinter(int tijdstap)
        {
            if (tijdstap == pKlaar)
            {
                Klant temp = q.Dequeue();

                //Bouwplaat naar Piet's Stack
                Program.piet.stack.Push(temp);
                temp.printklaar = tijdstap;

                //Volgende klant begint al met printen, als die klant er is
                if (q.Count > 0)
                    NieuweKlant(tijdstap);
            }
            //else if (!busy)  //geen klant aan het printen en er staat wel iemand in de rij
            //    if (q.Count > 0)
            //        NieuweKlant(tijdstap);
            //    else
            //        busy = false;
        }

        public void NieuweKlant(int tijdstap)
        {
            //Check welke klant het langst in de rij heeft gestaan
            if (tijdstap - q.First().binnenkomst > langstGewacht)
            {
                langstGewacht = tijdstap - q.First().binnenkomst;
                lGRegelNummer = q.First().lineNumber;
            }

            pKlaar = q.First().printTime + tijdstap;    //tijdstap tot wanneer de klant aan het printen is
        }
    }

    //Dit is Piet, piet bevat een stack en een methode om een tijdstap af te handelen
    public class Piet
    {
        public Stack<Klant> stack = new Stack<Klant>();
        int klaar = -1;
        public int klantCount, langstGewachtNaPrinten = -1, lGNPRegelNummer, langstGewachtNaBinnenkomst = -1, LGNBRegelNummer;
        Klant current;

        public Piet()
        {        }

        public void DoePiet(int tijdstap)
        {
            if(tijdstap == klaar)   //piet is klaar met snijden
            {
                if(tijdstap - current.printklaar > langstGewachtNaPrinten)
                {
                    langstGewachtNaPrinten = tijdstap - current.printklaar;
                    lGNPRegelNummer = current.lineNumber;
                }

                if(tijdstap - current.binnenkomst > langstGewachtNaBinnenkomst)
                {
                    langstGewachtNaBinnenkomst = tijdstap - current.binnenkomst;
                    LGNBRegelNummer = current.lineNumber;
                }

                klantCount--;

                if (stack.Count > 0)     //als er nog werk voor piet ligt begint hij weer met snijden
                    NieuwePlaat(tijdstap);
            }
            else if(klaar < tijdstap && stack.Count > 0)    //als piet niks aan het doen is maar er wel werk is begint hij met snijden
            {
                NieuwePlaat(tijdstap);
            }
        }

        public void NieuwePlaat(int tijdstap)   //Pop de stack en bereken de tijd wanneer Piet klaar is met snijden
        {
            current = stack.Pop();
            klaar = current.cutTime + tijdstap;
        }
    }
}
