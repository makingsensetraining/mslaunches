﻿using MSLunches.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSLunches.Domain.Services.Interfaces
{
    /// <summary>
    /// Service responsible of handling lunchs
    /// </summary>
    public interface IUserLunchService
    {
        /// <summary>
        /// Gets a userLunch by Id
        /// </summary>
        /// <param name="userLunchId">Id of the userLunch to be retrieved</param>
        /// <returns>A <see cref="Lunch"/> object if the userLunch is found, otherwise null</returns>
        Task<UserLunch> GetByIdAsync(Guid userLunchId);

        /// <summary>
        /// Creates a userLunch
        /// </summary>
        /// <param name="userLunch">Meal to create</param>
        /// <returns>An integer indicating the amount of affected rows</returns>
        Task<UserLunch> CreateAsync(UserLunch userLunch);

        /// <summary>
        /// Deletes a userLunch by Id
        /// </summary>
        /// <param name="lunchId">Id of the userLunch to delete</param>
        /// <returns>An integer indicating the amount of affected rows</returns>
        Task<int> DeleteByIdAsync(Guid lunchId);

        /// <summary>
        /// Gets all the existing lunchs
        /// </summary>
        /// <returns>List with all the existing lunchs</returns>
        Task<List<UserLunch>> GetAsync(string userId);

        /// <summary>
        /// Updates a userLunch
        /// </summary>
        /// <param name="userLunch">userLunch to update</param>
        /// <returns>An integer indicating the amount of affected rows</returns>
        Task<UserLunch> UpdateAsync(UserLunch userLunch);

        Task<List<UserLunch>> GetlLunchesByUserByWeekAsync(string userId);


        Task<UserLunch> GetUserLunchByUserAndLunchIdAsync(string userId, Guid lunchId);
    }
}