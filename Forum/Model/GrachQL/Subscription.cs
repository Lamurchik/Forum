using Forum.Model.DB;
namespace Forum.Model.GrachQL
{
    public class Subscription
    {
        [Subscribe]
        [Topic]
        public Comment OnCommentAdded(Comment comment)
        {
            return comment;
        }
    }


}

