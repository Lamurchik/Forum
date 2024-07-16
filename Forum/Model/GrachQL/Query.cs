using Forum.Model.DB;

namespace Forum.Model.GrachQL
{
    public class Query
    {
        public Comment GetComment()
        { 
            return new Comment() {UserId=0, PostId=0, Text="" };
        }
    }

}

