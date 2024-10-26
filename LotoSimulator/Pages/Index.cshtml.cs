using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace LotoSimulator.Pages
{
    public class IndexModel : PageModel
    {
        public byte[] RandomNumbers { get; private set; } = Array.Empty<byte>();
        public string SimulationResult { get; private set; } = string.Empty;


        public JsonResult OnPostGenerateNumbers()
        {
            var random = new Random();


            var ticketNumbers = new byte[7];
            int j = 0;
            while (j < 7)
            {
                byte newNumber = (byte)random.Next(1, 40);
                if (Array.IndexOf(ticketNumbers, newNumber, 0, j) == -1) // Proverava da li broj već postoji u nizu
                {
                    ticketNumbers[j++] = newNumber;
                }
            }

            RandomNumbers = ticketNumbers.OrderBy(n => n).ToArray();

            // Konvertujemo byte niz u string niz za slanje
            return new JsonResult(new { randomNumbers = RandomNumbers.Select(n => n.ToString()).ToArray() });
        }

        public JsonResult OnPostRunSimulation(int totalTickets)
        {



            var random = new Random();
            int count4 = 0, count5 = 0, count6 = 0, count7 = 0;

            // Generisanje tiketa sa slučajnim brojevima prema unetom broju
            var tickets = new byte[totalTickets][];
            for (int i = 0; i < totalTickets; i++)
            {
                var ticketNumbers = ArrayPool<byte>.Shared.Rent(7);
                int j = 0;
                while (j < 7)
                {
                    byte newNumber = (byte)random.Next(1, 40);
                    if (Array.IndexOf(ticketNumbers, newNumber, 0, j) == -1)
                    {
                        ticketNumbers[j++] = newNumber;
                    }
                }
                tickets[i] = ticketNumbers;
            }

            // Simulacija izvlačenja 7 slučajnih brojeva
            var drawNumbers = new byte[7];
            int k = 0;
            while (k < 7)
            {
                byte newNumber = (byte)random.Next(1, 40);
                if (Array.IndexOf(drawNumbers, newNumber, 0, k) == -1)
                {
                    drawNumbers[k++] = newNumber;
                }
            }
            Array.Sort(drawNumbers);


            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            // Paralelno brojanje pogodaka
            Parallel.For(0, totalTickets, parallelOptions, i =>
            {
                int matches = tickets[i].Count(n => drawNumbers.Contains(n));

                switch (matches)
                {
                    case 4:
                        Interlocked.Increment(ref count4);
                        break;
                    case 5:
                        Interlocked.Increment(ref count5);
                        break;
                    case 6:
                        Interlocked.Increment(ref count6);
                        break;
                    case 7:
                        Interlocked.Increment(ref count7);
                        break;
                }

                // Vraćanje niza u pool
                ArrayPool<byte>.Shared.Return(tickets[i]);
            });

            // Oslobađanje memorije
            Array.Clear(tickets, 0, tickets.Length);
            Array.Clear(drawNumbers, 0, drawNumbers.Length);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return new JsonResult(new
            {
                totalTickets,
                count4,
                count5,
                count6,
                count7
            });
        }






    }
}
