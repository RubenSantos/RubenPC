/*
 * INSTITUTO SUPERIOR DE ENGENHARIA DE LISBOA
 * Licenciatura em Engenharia Informática e de Computadores
 *
 * Programação Concorrente - Inverno de 2009-2010, Inverno de 1017-2018
 * Paulo Pereira, Pedro Félix
 *
 * Código base para a 3ª Série de Exercícios.
 *
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace Tracker
{
    /// <summary>
    /// Singleton class that hosts the dictionary
    /// 
    /// NOTE: This implementation is not thread-safe.
    /// </summary>
    public class Store
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static readonly Store _instance = new Store();
        private static readonly SafeReadWriteLock readWriteLock = new SafeReadWriteLock();

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static Store Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// The dictionary instance.
        /// </summary>
        private readonly Dictionary<string, string> _store;
                

        /// <summary>
        /// Initiates the store instance.
        /// </summary>
        private Store()
        {
            _store = new Dictionary<string, string>();            
        }

        /// <summary>
        /// Sets the key value. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string key, string value)
        {
            readWriteLock.lockWrite();
            _store[key] = value;
            readWriteLock.unlockWrite();
        }

        /// <summary>
        /// Gets the key value. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public string Get(string key)
        {
            string value = null;
            readWriteLock.lockRead();
            _store.TryGetValue(key, out value);
            readWriteLock.unlockRead();
            return value;
        }

        /// <summary>
        /// Gets all keys. 
        /// </summary>        
        public IEnumerable<string> Keys()
        {
            readWriteLock.lockRead();
            Dictionary<string, string>.KeyCollection keys = _store.Keys;
            readWriteLock.unlockRead();
            return keys;
        }
    }

    class SafeReadWriteLock
    {
        // to correct publishing the variable is marked as volatile
        private volatile int state;

        /// <summary>
        /// This is the classic lock aquisition
        /// </summary>
        public void lockWrite()
        {
            while (Interlocked.CompareExchange(ref state, 1, 0) != 0)
                Thread.Yield();
        }

        /// <summary>
        /// Just publish 0 on state
        /// </summary>
        public void unlockWrite()
        {
            state = 0;
        }

        /// <summary>
        /// A reader is comming. Increment state if possible
        /// </summary>
        public void lockRead()
        {
            do
            {
                int obs = state;
                if (obs < 0) Thread.Yield();
                else if (Interlocked.CompareExchange(ref state, obs + 1, obs) == obs)
                    return;
            }
            while (true);
        }

        /// <summary>
        /// This implementation check for an invalid call, i.e., the state mus be positive
        /// on a legal call
        /// </summary>
        public void unlockRead()
        {
            int obs;
            do
            {
                obs = state;
                if (obs <= 0)
                    throw new InvalidOperationException();
            }
            while ((Interlocked.CompareExchange(ref state, obs - 1, obs) != obs));
        }
    }
}
