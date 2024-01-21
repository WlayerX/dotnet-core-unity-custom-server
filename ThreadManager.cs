using System;
using System.Collections.Generic;

class ThreadManager
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;

    // Ana iş parçacığında yürütülecek eylemi belirler.
    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            Console.WriteLine("Hata: Ana iş parçacığında yürütülecek bir eylem bulunamadı!");
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }

    // Ana iş parçacığında çalışması gereken tüm kodları yürütür. NOT: Bu yalnızca ana iş parçacığından çağrılmalıdır.
    public static void UpdateMain()
    {
        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            foreach (var action in executeCopiedOnMainThread)
            {
                action();
            }
        }
    }
}
