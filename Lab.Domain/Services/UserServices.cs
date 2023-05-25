﻿using Common.Exceptions;
using Common.Helpers;
using Common.Resources;
using Infraestructure.Core.UnitOfWork.Interface;
using Infraestructure.Entity.Models;
using Lab.Domain.Dto;
using Lab.Domain.Dto.User;
using Lab.Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Common.Constant.Const;

namespace Lab.Domain.Services
{
    public class UserServices : IUserServices
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public UserServices(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }


        #region authentication

        public TokenDto Login(LoginDto login)
        {
            UserEntity user = _unitOfWork.UserRepository.FirstOrDefault(x => x.Mail == login.UserName,
                                                                          r => r.RoleEntity);
            if (user == null)
                throw new BusinessException("El usuario ingresado no existe");

            string userEnteredPassword = login.Password;
            string hashedPasswordFromDatabase = user.Password;

            if (!Common.Helpers.Utils.VerifyPassword(userEnteredPassword, hashedPasswordFromDatabase))
                throw new BusinessException(GeneralMessages.PasswordIncorrect);

            return GenerateTokenJWT(user);

        }

        public TokenDto GenerateTokenJWT(UserEntity userEntity)
        {
            IConfigurationSection tokenAppSetting = _configuration.GetSection("Tokens");

            var _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenAppSetting.GetSection("Key").Value));
            var _signingCredentials = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var _header = new JwtHeader(_signingCredentials);

            var _Claims = new[] {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(TypeClaims.IdUser,userEntity.Id.ToString()),
                new Claim(TypeClaims.Email,userEntity.Mail),
                new Claim(TypeClaims.IdRol,userEntity.IdRole.ToString())

            };

            var _payload = new JwtPayload(
                    issuer: tokenAppSetting.GetSection("Issuer").Value,
                    audience: tokenAppSetting.GetSection("Audience").Value,
                    claims: _Claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(60)
                );

            var _token = new JwtSecurityToken(
                    _header,
                    _payload
                );

            TokenDto token = new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(_token),
                Expiration = Utils.ConvertToUnixTimestamp(_token.ValidTo),
            };
            return token;
        }

        #endregion

        private UserEntity Get(int idUser) => _unitOfWork.UserRepository.FirstOrDefault(x => x.Id == idUser);


        public List<GetUserDto> GetAll()
        {
            IEnumerable<UserEntity> userQuery = _unitOfWork.UserRepository.GetAll(x => x.RoleEntity);
            //.FindAll(x=> & | ) para separar las condiciones


            List<GetUserDto> usuarios = userQuery.Select(x => new GetUserDto()
            {
                Id = x.Id,
                Email = x.Mail,
                Password = x.Password,
                IdRole = x.IdRole,
                Role = x.RoleEntity.Description,
                IsActive = x.IsActive
            })
            .ToList();

            return usuarios;

        }

        public async Task<bool> Insert(AddUserDto dto)
        {
            if (_unitOfWork.UserRepository.FirstOrDefault(x => x.Mail == dto.Email) != null)
                throw new BusinessException(GeneralMessages.RegisteredEmail);

            UserEntity user = new UserEntity()
            {
                Mail = dto.Email,
                Password = Common.Helpers.Utils.PassEncrypt(dto.Password),
                IsActive = true,
                IdRole = dto.IdRole
            };
            _unitOfWork.UserRepository.Insert(user);

            return await _unitOfWork.Save() > 0;
        }


        public async Task<bool> Delete(int id)
        {
            UserEntity? userEntity = _unitOfWork.UserRepository.FirstOrDefault((user) => user.Id == id);

            if (userEntity == null)
                throw new BusinessException(GeneralMessages.ItemNoFound);

            userEntity.IsActive = false;
            _unitOfWork.UserRepository.Update(userEntity);

            return await _unitOfWork.Save() > 0;
        }


        public async Task<bool> UpdatePassword(UserPasswordDto password, int idUser)
        {
            UserEntity userExist= Get(idUser);
            if (userExist == null)
                throw new BusinessException(GeneralMessages.ItemNoFound);

            string userEnteredPassword = password.Password;
            string hashedPasswordFromDatabase = userExist.Password;

            if (!Utils.VerifyPassword(userEnteredPassword, hashedPasswordFromDatabase))
                throw new BusinessException(GeneralMessages.PasswordIncorrect);

            userExist.Password = Common.Helpers.Utils.PassEncrypt(password.NewPassword);
            _unitOfWork.UserRepository.Update(userExist);

            return await _unitOfWork.Save() > 0;
        }


        public async Task<bool> Update(TokenDto tokenDto, string newPassword)
        {
            //JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            //JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(tokenDto.Token);

            //Claim emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TypeClaims.Email);
            //if (emailClaim == null)
            //    throw new BusinessException("El token no contiene la información necesaria");


            //UserEntity? userExistente = _unitOfWork.UserRepository.FindAll((user) => user.Mail == emailClaim.Value).SingleOrDefault();

            //if (userExistente != null && Common.Helpers.Utils.IsValidPassword(newPassword))
            //{
            //    userExistente.Password = Common.Helpers.Utils.PassEncrypt(newPassword);
            //    _unitOfWork.UserRepository.Update(userExistente);
            //    return await _unitOfWork.Save() > 0;

            //}


            return false;
        }
    }

}
