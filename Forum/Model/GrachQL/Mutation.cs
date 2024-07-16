using HotChocolate.Subscriptions;
using Forum.Model.DB;
namespace Forum.Model.GrachQL
{
    public class Mutation
    {
        private readonly ITopicEventSender _eventSender;

        public Mutation(ITopicEventSender eventSender)
        {
            _eventSender = eventSender;
        }

        public async Task<Comment> AddComment(Comment comment)
        {
            

            // Отправка события о новом комментарии
            await _eventSender.SendAsync($"OnCommentAdded_{comment.PostId}", comment);

            return comment;
        }
    }


}

