﻿using Application.Profiles;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Profile = Application.Profiles.Profile;

namespace Application.Followers
{
    public class List
    {
        public class Query : IRequest<List<Profile>>
        {
            public string Username { get; set; }
            public string Predicate { get; set; }
        }

        public class Handler : IRequestHandler<Query, List<Profile>>
        {
            private readonly DataContext context;
            private readonly IProfileReader profileReader;

            public Handler(DataContext context, IProfileReader profileReader)
            {
                this.context = context;
                this.profileReader = profileReader;
            }

            public async Task<List<Profile>> Handle(Query request, CancellationToken cancellationToken)
            {
                var queryable = context.Followings.AsQueryable();

                var userFollowings = new List<UserFollowing>();
                var profiles = new List<Profile>();

                switch (request.Predicate)
                {
                    case "follower":
                        {
                            userFollowings = await queryable
                                .Where(x => x.Target.UserName == request.Username)
                                .ToListAsync();

                            foreach (var follower in userFollowings)
                            {
                                profiles.Add(await profileReader.ReadProfile(follower.Observer.UserName));
                            }

                            break;
                        }
                    case "following":
                        {
                            userFollowings = await queryable
                                .Where(x => x.Observer.UserName == request.Username)
                                .ToListAsync();

                            foreach (var follower in userFollowings)
                            {
                                profiles.Add(await profileReader.ReadProfile(follower.Target.UserName));
                            }

                            break;
                        }
                }

                return profiles; 
            }
        }
    }
}
