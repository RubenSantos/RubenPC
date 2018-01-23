using Aula_2017_11_30;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Estudo
{
    /**
     4.
    A interface Services define os serviços síncronos disponibilizados por uma organização
    que oferece a execução de diversos serviços em servidores localizados em diferentes áreas
    geográficas (SaaS). O método PingServer responde a um pedido de ping, devolvendo o Uri do
    respectivo servidor. O método ExecService executa, no servidor especificado através do
    parâmetro server, o serviço especificado pelos tipos genéricos S (serviço) e R (resposta).
    O método ExecOnNearServer usa as operações de Services para executar de forma síncrona o
    serviço especificado, no servidor que primeiro responder ao serviço PingServer, isto é,
    aquele que se considera ser o servidor mais próximo.

    public class Exec{
	    public interface Services<S,R> {
		    Uri PingServer(Uri server);
		    R ExecService(Uri server,S service);
	    }

	    public R ExecOnNearServer<S,R>(Services<S,R> svc, Uri[] servers, S service);
    }

    a.
    A classe APMExec será a variante assíncrona de Exec ao estilo Asynchronous Programming Model (APM).
    Implemente os métodos BeginExecOnNearServer e EndExecOnNearServerque usam a interface APMServices
    (variante APM de Services que não tem de apresentar).

    NOTA: não pode usar a TPL e só se admitem esperas de controlo dentro das operações End,
    estritamente onde o APM o exige.

    b.
    A classe TAPExec será a variante assíncrona de Exec, ao estilo Task based Asynchronous Pattern (TAP).
    Usando a funcionalidade oferecida pela Task Parallel Library (T P L) ou pelos métodos async do C#,
    implemente o método ExecOnNearServerAsync, que usa a interface TAPServices (variante TAP de Services
    que não tem de apresentar).
    NOTA: na implementação não se admite a utilização de operações com bloqueios de controlo.
    **/
    class PC_1516v_1_Ex4e5
    {
    }

    public class APMExec
    {
        public interface IAPMServices<S, R>
        {
            IAsyncResult BeginPingServer(Uri server, AsyncCallback ucb, object state);
            Uri EndPingServer(IAsyncResult ar);
            IAsyncResult BeginExecService(Uri server, S service, AsyncCallback ac, object state);
            R EndExecService(IAsyncResult ar);
        }

        public IAsyncResult BeginExecOnNearServer<S,R>(IAPMServices<S,R> s, Uri[] servers, S service,
            AsyncCallback ac, object state)
        {
            GenericAsyncResult<R> gar = new GenericAsyncResult<R>(ac, state, false);
            int got = 0, failures = 0;
            for (int i = 0; i < servers.Length; i++)
                s.BeginPingServer(servers[i], onPingServer, null);
            void onPingServer(IAsyncResult ar)
            {
                try
                {
                    Uri uri = s.EndPingServer(ar);
                    if (Interlocked.Exchange(ref got, 1) == 0)
                        s.BeginExecService(uri, service, onExecService, null);
                }
                catch (Exception ex)
                {
                    if (Interlocked.Increment(ref failures) == servers.Length)
                        gar.SetException(ex);
                }
            }
            void onExecService(IAsyncResult ar)
            {
                try
                {
                    gar.SetResult(s.EndExecService(ar));
                }
                catch(Exception ex)
                {
                    gar.SetException(ex);
                }
            }
            return gar;
        }

        public R EndExecOnNearServer<R>(IAsyncResult ar)
        {
            return ((GenericAsyncResult<R>)ar).Result;
        }
    }

    public class TAPExec
    {
        public interface ITAPServices<S,R>
        {
            Task<Uri> PingServerAsync(Uri server);
            Task<R> ExecServiceAsync(Uri server, S service);
        }

        public Task<R> ExecOnNearServerAsync<S, R>(ITAPServices<S, R> svc, Uri[] servers, S service)
        {
            Task<Uri>[] pingTasks = new Task<Uri>[servers.Length];
            for (int i = 0; i < servers.Length; ++i)
                pingTasks[i] = svc.PingServerAsync(servers[i]);
            return Task.Factory.ContinueWhenAny(pingTasks, (ant) =>
            {
                pingTasks = pingTasks.Where(t => t != ant).ToArray();
                Task.Factory.ContinueWhenAll(pingTasks, (ant2) =>
                {
                    try
                    {
                        Task.WaitAll(ant2);
                    }
                    catch { }
                });

                return svc.ExecServiceAsync(ant.Result, service);
            }).Unwrap();
        }

        public async Task<R> ExecOnNearServerAsync_4<S, R>(ITAPServices<S, R> svc, Uri[] servers, S service)
        {
            Task<Uri>[] tasks = new Task<Uri>[servers.Length];
            for (int i = 0; i < servers.Length; ++i)
                tasks[i] = svc.PingServerAsync(servers[i]);
            Task.Factory.ContinueWhenAll(tasks, (antecedents) => {
                try { Task.WaitAll(antecedents); } catch { /*** log any thrown exceptions ***/ }
            });
            return await svc.ExecServiceAsync(await Task.WhenAny(tasks).Result, service);

        }

        public async Task<R> ExecOnNearServerAsync_5<S, R>(ITAPServices<S, R> svc, Uri[] servers, S service)
        {
            Task<Uri>[] pingTasks = new Task<Uri>[servers.Length];
            for (int i = 0; i < servers.Length; ++i)
                pingTasks[i] = svc.PingServerAsync(servers[i]);
            do
            {
                Task<Uri> pingTask = await Task.WhenAny(pingTasks);
                pingTasks = pingTasks.Where(t => t != pingTask).ToArray();
                try
                {
                    Uri uri = pingTask.Result;
                    Task.Factory.ContinueWhenAll(pingTasks, (ant) =>
                    {
                        try
                        {
                            Task.WaitAll(ant);
                        }
                        catch
                        {

                        }
                    });
                    return await svc.ExecServiceAsync(uri, service);
                }
                catch (AggregateException ae)
                {
                    if (pingTasks.Length == 0)
                        throw ae.InnerException;
                }
            } while (true);
        }
    }

    /***
    5.
    No método MapAggregate, apresentado a seguir, as invocações a Map podem decorrer em paralelo, o que
    seria vantajoso já que é nessa operação se concentra a maior componente de processamento. O método
    Aggregate implementa a operação, comutativa e associativa, que agrega os resultados, sendo o respectivo
    elemento neutro produzindo pela expressão new Result(). A operação de agregação pode retornar overflow,
    situação em que o método MapAggregatedeverá retornar rapidamente essa indicação. Tirando partido da
    Task Parallel Library, apresente uma versão do método MapAggregate que faça invocações paralelas ao método
    Map de modo a tirar partido de todos os cores de processamento disponíveis.
    NOTA: considere que o método  Aggregate retorna  Result.OVERFLOW quando um dos seus argumentos já for
    overflow.
 
    public static Result MapAggregate(IEnumerable<Data> data) {
	    Result result = newResult();
	    foreach (var datum in data) {
		    result = Aggregate(result, Map(datum));
		    if (result.Equals(Result.OVERFLOW))
			    break;
	     }
	    return result;
    }

    ***/
    class ParallelMapAggregateClass
    {
        internal class Result
        {
            internal int value;
            internal static Result OVERFLOW = new Result(-1);
            internal Result(int v)
            {
                value = v;
            }
            internal Result() : this(0) { }
        }

        internal class Data
        {
            internal int value;

            internal Data(int v) { value = v; }
        }

        internal static Result Map(Data datum)
        {
            Thread.SpinWait(100000);
            return new Result(datum.value);
        }

        internal static Result Aggregate(Result r, Result r2)
        {
            if (r.value > 50000)
                return Result.OVERFLOW;
            return new Result(r.value + r2.value);
        }

        public static Result ParallelMapAggregate(IEnumerable<Data> data)
        {
            Result total = new Result();
            bool stop = false;
            object monitor = new object();
            Parallel.ForEach(data,
                () => new Result(),
                (datum, loopState, partial) =>
                {
                    if (Volatile.Read(ref stop))
                        return partial;
                    partial = Aggregate(partial, Map(datum));
                    if (partial.Equals(Result.OVERFLOW))
                    {
                        Volatile.Write(ref stop, true);
                        loopState.Stop();
                    }
                    return partial;
                },
                (partial) =>
                {
                    lock (monitor)
                    {
                        total = Aggregate(total, partial);
                        if (total.Equals(Result.OVERFLOW))
                            Volatile.Write(ref stop, true);
                    }
                });
            return total;
        }
    }
}
