using PCSC;
using System;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                {
                    var readerNames = context.GetReaders(); if (readerNames == null || readerNames.Length < 1) return;

                    bool Readed = false;
                    int TryCount = 0;
                    while ((!Readed) && TryCount <= 10)
                    {
                        TryCount++;

                        foreach (var readerName in readerNames)
                        {
                            
                            try
                            {
                                using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                                {

                                    var atr = reader.GetStatus().GetAtr();
                                    if (atr != null && atr.Length > 0)
                                    {
                                        Readed = true;
                                        Console.WriteLine(BitConverter.ToString(atr));
                                        break;
                                    }
                                }
                            } catch { }

                        }
                        Thread.Sleep(500);
                    }
                }
            }
            catch { }

            return;
        }
    }
}
