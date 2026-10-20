using NEAProjectMain.SQLSelectClasses;
using NEAProjectMain;

namespace NEAUnitTests.Tests
{
    [TestClass]

 //am using to test edgecases for my merge sort
    public class UnitTest1
    {


        [TestMethod]
        public void MergeSort_JumbledList()
        {


            var jumbledList = new List<MonthlyReturnCount>
            {
                new MonthlyReturnCount { Month = 5 }, 
                new MonthlyReturnCount { Month = 1 },   
                new MonthlyReturnCount { Month = 12 }, 
                new MonthlyReturnCount { Month = 7 },   
                new MonthlyReturnCount { Month = 3 }  
            };


            var result = MainGUI.SplitListIntoTwoLists(jumbledList);

            //checks against expected values
            Assert.AreEqual(12, result[0].Month); 
            Assert.AreEqual(7, result[1].Month);  
            Assert.AreEqual(5, result[2].Month);  
            Assert.AreEqual(3, result[3].Month);  
            Assert.AreEqual(1, result[4].Month);  
        }




        [TestMethod]
        public void MergeSort_SingleItem()
        {

            var singleItemList = new List<MonthlyReturnCount>
            {

                new MonthlyReturnCount { Month = 6 }  
            };

            var result = MainGUI.SplitListIntoTwoLists(singleItemList);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(6, result[0].Month);
        }





        [TestMethod]
        public void MergeSort_EmptyList()
        {
            var emptyList = new List<MonthlyReturnCount>();



            var result = MainGUI.SplitListIntoTwoLists(emptyList);


            Assert.AreEqual(0, result.Count);
        }



    }
}