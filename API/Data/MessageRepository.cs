using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
   public class MessageRepository : IMessageRepository
   {
      private readonly DataContext _context;
      private readonly IMapper _mapper;
      public MessageRepository(DataContext context, IMapper mapper)
      {
         _mapper = mapper;
         _context = context;
      }

      public void AddGroup(Group group)
      {
         _context.Groups.Add(group);
      }

      public void AddMessage(Message message)
      {
         _context.Messages.Add(message);
      }

      public void DeleteMessage(Message message)
      {
         _context.Messages.Remove(message);
      }

      public async Task<Connection> GetConnection(string connectionId)
      {
         return await _context.Connections.FindAsync(connectionId);
      }

      public async Task<Group> GetGroupForConnection(string connectionId)
      {
         return await _context.Groups
                           .Include(o => o.Connections)
                           .Where(o => o.Connections.Any(p => p.ConnectionId == connectionId))
                           .FirstOrDefaultAsync();
      }

      public async Task<Message> GetMessage(int id)
      {
         return await _context.Messages.Include(o => o.Sender).Include(o => o.Recipient).SingleOrDefaultAsync(o => o.Id == id);
      }

      public async Task<Group> GetMessageGroup(string groupName)
      {
         return await _context.Groups.Include(o => o.Connections).FirstOrDefaultAsync(o => o.Name == groupName);
      }

      public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
      {
         var query = _context.Messages.OrderBy(m => m.MessageSent).AsQueryable();

         query = messageParams.Container switch
         {
            "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false),
            "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username && u.SenderDeleted == false),
            _ => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
         };

         var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

         return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
      }

      public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
      {
         var messages = await _context.Messages
         .Include(o => o.Sender).ThenInclude(o => o.Photos)
         .Include(o => o.Recipient).ThenInclude(o => o.Photos)
         .Where(o => o.Recipient.UserName == currentUsername && o.RecipientDeleted == false && o.Sender.UserName == recipientUsername
         || o.Recipient.UserName == recipientUsername && o.Sender.UserName == currentUsername && o.SenderDeleted == false)
         .OrderBy(o => o.MessageSent)
         .ToListAsync();

         var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUsername).ToList();
         if (unreadMessages.Any())
         {
            foreach (var message in unreadMessages)
            {
               message.DateRead = DateTime.UtcNow;
            }
         }
         await _context.SaveChangesAsync();

         return _mapper.Map<IEnumerable<MessageDto>>(messages);
      }

      public void RemoveConnection(Connection connection)
      {
         _context.Connections.Remove(connection);
      }

      public async Task<bool> SaveAllAsync()
      {
         return await _context.SaveChangesAsync() > 0;
      }
   }
}