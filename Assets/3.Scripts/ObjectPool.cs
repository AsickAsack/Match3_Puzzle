using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool<T> where T : class
{

    Queue<T> pool = new Queue<T>();
    Func<T> creatFunc;
    Action<T> initAction;
    Action<T> returnAction;

    //생성자
    public ObjectPool(int poolSize,Func<T> createFunc, Action<T> InitAction, Action<T> returnAction)
    {
        this.creatFunc = createFunc;
        this.initAction = InitAction;
        this.returnAction = returnAction;   

        for ( int i = 0; i < poolSize; i++ )
        {
            pool.Enqueue(createFunc());
        }
    }
    
    //빌려줄때
    public T BorrowFromPool()
    {
        T newObj = null;

        if (pool.Count.Equals(0))
        {
            newObj = creatFunc();
        }
        else
        {
            newObj = pool.Dequeue();
        }

        initAction(newObj);

        return newObj;
    }

    //되돌려 받을때
    public void ReturnToPool(T returnObj)
    {
        returnAction(returnObj);
        pool.Enqueue(returnObj);
    }


}
