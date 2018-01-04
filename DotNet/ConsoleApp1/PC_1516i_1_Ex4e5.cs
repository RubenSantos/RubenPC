using Aula_2017_11_30;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Estudo
{

    /**
    4.
    A interface Users.Service disponibiliza serviços base de um sistema de gestão de utilizadores,
    apenas em versões assíncronas, quer no modelo Asynchronous Programming Model (APM), quer no
    modelo Task-based Asynchronous Pattern (TAP). A classe Users disponibiliza ainda a operação
    composta GetUserAvatarAsync, também assíncrona, que invoca dois serviços assíncronos em
    sequência. No entanto, verifica-se que o uso desta operação resulta num consumo considerável
    de recursos nos servidores do sistema, principalmente nos momentos em que há mais pedidos em
    curso simultaneamente. 

	    public class Users {
		    public interface Service {
			    IAsyncResult BeginFindId(String name, String bdate, AsyncCallback cb, Object stt);
			    int EndFindId(IAsyncResult asyncRes);
			    Task<int> FindIdAsync(String name, String birthdate);
			    IAsyncResult BeginObtainAvatarUri(int userId, AsyncCallback cb, Object stt);
    		    Uri EndObtainAvatarUri(IAsyncResult asyncRes);
    		    Task<Uri> ObtainAvatarUriAsync(int userId);
		    }
  
  		    public static Task<Uri> GetUserAvatarAsync(Service svc, String name, String bdate) {
    		    return Task.Run(() => {
      				    int userId = service.FindIdAsync(name, bdate).Result;
      				    return service.ObtainAvatarUriAsync(userId).Result;
    		    });
  		    }
	    }

    a.
    Sabendo que as operações de Users.Service consistem essencialmente em I/O, qual é o defeito
    grave de implementação de GetUserAvatarAsync e porque razão este impõe um elevado consumo de
    recursos quando é intensivamente utilizado em vários pedidos em simultâneo?

    b.
    Apresente uma implementação corrigida de GetUserAvatarAsync, utilizando devidamente a
    Task Parallel Library (TPL) e/ou os métodos async de C#.

    c.
    Acrescente à classe Users as operações BeginGetUserAvatar e EndGetUserAvatar, para que a
    operação GetUserAvatar também possa ser utilizada de acordo com o modelo APM.
    NOTA: não pode usar a TPL e só se admitem esperas de controlo dentro da operação End,
    estritamente onde o APM o exige.

    **/

    class PC_1516i_1_Ex4e5
    {
        public static void Main()
        {
            Users.TestTAP();
            Users.TestAPM();
            DoParallel.Test();
        }
    }

    public class Users
    {
        public interface Service
        {
            IAsyncResult BeginFindId(String name, String bdate, AsyncCallback cb, Object stt);
            int EndFindId(IAsyncResult asyncRes);

            Task<int> FindIdAsync(String name, String birthdate);
            IAsyncResult BeginObtainAvatarUri(int userId, AsyncCallback cb, Object stt);

            Uri EndObtainAvatarUri(IAsyncResult asyncRes);
            Task<Uri> ObtainAvatarUriAsync(int userId);
        }

        public class ServiceImpl : Service
        {
            IAsyncResult Service.BeginFindId(String name, String bdate, AsyncCallback cb, Object st)
            {
                GenericAsyncResult<int> ar = new GenericAsyncResult<int>(cb, st, false);
                new Timer(
                  (_) => ar.SetResult(42), null,
                  new Random(Environment.TickCount).Next(1000),
                    Timeout.Infinite
                );
                return ar;
            }
            int Service.EndFindId(IAsyncResult asyncRes)
            {
                return ((GenericAsyncResult<int>)asyncRes).Result;
            }

            Task<int> Service.FindIdAsync(String name, String birthdate)
            {
                Thread.Sleep(new Random(Environment.TickCount).Next(1000));
                return Task<int>.FromResult(42);
            }

            IAsyncResult Service.BeginObtainAvatarUri(int userId, AsyncCallback cb, Object st)
            {
                GenericAsyncResult<Uri> ar = new GenericAsyncResult<Uri>(cb, st, false);
                new Timer(
                  (_) => ar.SetResult(new Uri("http://google.com")),
                    null,
                  new Random(Environment.TickCount).Next(1000),
                    Timeout.Infinite
                );
                return ar;
            }

            Uri Service.EndObtainAvatarUri(IAsyncResult asyncRes)
            {
                return ((GenericAsyncResult<Uri>)asyncRes).Result;
            }

            Task<Uri> Service.ObtainAvatarUriAsync(int userId)
            {
                Thread.Sleep((new Random(Environment.TickCount)).Next(2000));
                return Task<Uri>.FromResult(new Uri("http://google.com"));
            }
        }

        public static Task<Uri> GetUserAvatarAsync(Service svc, String name, String bdate)
        {
            return Task.Run(() => {
                int userId = svc.FindIdAsync(name, bdate).Result;
                return svc.ObtainAvatarUriAsync(userId).Result;
            });
        }

        public static Task<Uri> GetUserAvatarAsync_b(Service svc, string name, string bdate)
        {
            return svc.FindIdAsync(name, bdate).ContinueWith((_ant) => svc.ObtainAvatarUriAsync(_ant.Result)).Unwrap();
        }

        public async static Task<Uri> GetUserAvatarAsync_bb(Service svc, string name, string bdate)
        {
            return await svc.ObtainAvatarUriAsync(await svc.FindIdAsync(name, bdate));
        }

        public static void TestTAP()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var avatarUriTask = GetUserAvatarAsync_bb(new ServiceImpl(), "name", "date");
            avatarUriTask.Wait();
            Console.WriteLine("--TAP: elapsed {0} ms, got: \"{1}\"",
                               sw.ElapsedMilliseconds, avatarUriTask.Result);
        }

        public static IAsyncResult BeginGetUserAvatar(Service svc, string name, string bdate, AsyncCallback cb, object state)
        {
            GenericAsyncResult<Uri> gar = new GenericAsyncResult<Uri>(cb, state, false);
            AsyncCallback onObtainAvatarUri = (_ant) =>
            {
                var uri = svc.EndObtainAvatarUri(_ant);
                gar.SetResult(uri);
            };

            AsyncCallback onFindId = (_ant) =>
            {
                int id = svc.EndFindId(_ant);
                svc.BeginObtainAvatarUri(id, onObtainAvatarUri, null);
            };

            svc.BeginFindId(name, bdate, onFindId, null);
            return gar;
        }

        public static Uri EndGetUserAvatar(IAsyncResult ar)
        {
            return ((GenericAsyncResult<Uri>)ar).Result;
        }

        public static void TestAPM()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var ar = BeginGetUserAvatar(new ServiceImpl(), "name", "date", null, null);
            var avatarUri = EndGetUserAvatar(ar);
            Console.WriteLine("--APM: elapsed {0} ms, got: \"{1}\"",
                               sw.ElapsedMilliseconds, avatarUri);
        }

    }

    public class DoParallel
    {
        class Info { public int count; }
        class Data { public int value; }
        class Session { }

        static Info ExtractInfo(Data d, Session s)
        {
            return new Info { count = d.value };
        }

        static Info MergeInfo(Info i, Info i2)
        {
            return new Info { count = i.count + i2.count };
        }

        static Info ProcessItems(Data[] items, Session session)
        {
            Info info = new Info();
            for (int i = 0; i < items.Length; ++i)
                info = MergeInfo(info, ExtractInfo(items[i], session));
            return info;
        }

        //the best!

        static Info ProcessItemsParallel(Data[] items, Session session)
        {
            Info total = new Info();
            object monitor = new object();
            Parallel.ForEach<Data, Info>(items,
                () => new Info(),
                (item, loopState, partial) =>
                {
                    return MergeInfo(partial, ExtractInfo(item, session));
                },
                (partial) =>
                {
                    lock (monitor)
                    {
                        total = MergeInfo(total, partial);
                    }
                }
            );
            return total;
        }

        static Info ProcessItemsParallel2(Data[] items, Session session)
        {
            var workers = new List<Task<Info>>();
            for (int i = 0; i < items.Length; ++i)
            {
                int li = i;
                workers.Add(Task.Run(() => ExtractInfo(items[li], session)));
            }
            Task.WaitAll(workers.ToArray());
            Info total = new Info();
            foreach (var t in workers)
                total = MergeInfo(total, t.Result);
            return total;
        }

        static Info ProcessItemsParallel_3(Data[] items, Session session)
        {
            return Task.WhenAll(items.Select(item => Task.Run(() => ExtractInfo(item, session)))).Result.Aggregate(MergeInfo);
        }

        static async Task<Info> ProcessItemsParallel_4(Data[] items, Session session)
        {
            var workers = new List<Task<Info>>();
            for(int i = 0; i<items.Length; ++i)
            {
                int li = i;
                workers.Add(Task.Run(() => ExtractInfo(items[li], session)));
            }
            var results = await Task.WhenAll(workers.ToArray());
            Info total = new Info();
            for (int i = 0; i < results.Length; ++i)
                total = MergeInfo(total, results[i]);
            return total;
        }

        public static void Test()
        {
            Data[] items = new Data[32];
            for (int i = 0; i < 32; i++)
                items[i] = new Data { value = i + 1 };
            var result = ProcessItemsParallel_3(items, new Session());
            Console.WriteLine("result: {0}", result.count);
        }
    }
}
