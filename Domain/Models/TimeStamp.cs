
namespace VRefSolutions.Domain.Models
{
    // TimeOnly is not the correct class for the job, a simple custom class does the trick.  
    public class TimeStamp
    {
        
        public int Hours { get; set; }

        
        public int Minutes { get; set; }

        
        public int Seconds { get; set; }

        
        public int Miliseconds { get; set; }


        public TimeStamp(int Hours, int Minutes, int Seconds, int Miliseconds)
        {
            this.Hours = Hours;
            this.Minutes = Minutes;
            this.Seconds = Seconds;
            this.Miliseconds = Miliseconds;
        }

        public static TimeStamp Parse(string timestamp, char seperator)
        {
            // format "xx:yy:zz"
            string[] splittedValues = timestamp.Split(seperator);

            int Hours = int.Parse(splittedValues[0]);
            int Minutes = int.Parse(splittedValues[1]);
            int Seconds = int.Parse(splittedValues[2]);
            int Miliseconds = 0;
            if(splittedValues.Length > 3)
                Miliseconds = int.Parse(splittedValues[3]);
            return new TimeStamp(Hours, Minutes, Seconds, Miliseconds);
        }

        public bool IsWithinRange(TimeStamp compareTime, int range)
        {
            int totalSeconds = TotalSeconds();
            int compareTimeTotalSeconds = compareTime.TotalSeconds();

            bool isLargerThanTimeMinusRange = totalSeconds >= compareTimeTotalSeconds - range;
            bool isSmallerThanTimePlusRange = totalSeconds <= compareTimeTotalSeconds + range;

            return isLargerThanTimeMinusRange && isSmallerThanTimePlusRange;
        }

        public int TotalSeconds()
        {
            return (Hours * 3600) + (Minutes * 60) + Seconds;
        }

        public bool Equals(TimeStamp compareObj)
        {
            return compareObj.Hours == Hours 
                && compareObj.Minutes == Minutes 
                && compareObj.Seconds == Seconds
                && compareObj.Miliseconds == Miliseconds;
        }

        public override string ToString()
        {
            return string.Join(':', new int[] { Hours, Minutes, Seconds, Miliseconds });
        }

        public string ToString(char separator)
        {
            return string.Join(separator, new int[] { Hours, Minutes, Seconds, Miliseconds });
        }

    }

}