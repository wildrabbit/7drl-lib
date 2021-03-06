using System;
using System.Collections.Generic;

public static class CollectionUtils
{
    static public List<TResult> GetImplementors<TInput, TResult>(List<TInput> collection)
    {
        return collection.FindAll(x => typeof(TResult).IsAssignableFrom(x.GetType())).ConvertAll(x => (TResult)(object)x);
    }
}
