using System;
using System.Diagnostics;

namespace sakuragram.Services;

public static class MathService
{
    public static DateTime CalculateDateTime(int time)
    {
        try
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(time).ToLocalTime();

            return dateTime;
        } 
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return DateTime.MinValue;
        }
    }
}