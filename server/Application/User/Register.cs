﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Validators;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User {
    public class Register {
        public class Command : IRequest<User> {
            public string DisplayName { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command> {
            public CommandValidator () {
                RuleFor (x => x.DisplayName).NotEmpty ();
                RuleFor (x => x.Username).NotEmpty ();
                RuleFor (x => x.Email).NotEmpty ().EmailAddress ();
                RuleFor (x => x.Password).Password ();
            }
        }

        public class Handler : IRequestHandler<Command, User> {
            private readonly DataContext context;
            private readonly UserManager<AppUser> userManager;
            private readonly IJwtGenerator jwtGenerator;

            public Handler (DataContext context, UserManager<AppUser> userManager, IJwtGenerator jwtGenerator) {
                this.context = context;
                this.userManager = userManager;
                this.jwtGenerator = jwtGenerator;
            }
            public async Task<User> Handle (Command request, CancellationToken cancellationToken) {
                if (await context.Users.Where (x => x.Email == request.Email).AnyAsync ()) {
                    throw new RestException(HttpStatusCode.BadRequest, new { user = "This email is already registered" });
                }
                if (await context.Users.Where (x => x.UserName == request.Username).AnyAsync ()) {
                    throw new RestException(HttpStatusCode.BadRequest, new { user = "This username is already used" });
                }
                var user = new AppUser {
                    DisplayName = request.DisplayName,
                    Email = request.Email,
                    UserName = request.Username,
                };

                var result = await userManager.CreateAsync (user, request.Password);

                if (result.Succeeded)
                {
                    return new User
                    {
                        DisplayName = user.DisplayName,
                        Token = jwtGenerator.CreateToken(user),
                        Username = user.UserName,
                        Image = user.Photos?.FirstOrDefault(x => x.IsMain)?.Url
                    };
                }

                // Cannot create user
                throw new Exception ("Problem creating user");
            }
        }
    }
}