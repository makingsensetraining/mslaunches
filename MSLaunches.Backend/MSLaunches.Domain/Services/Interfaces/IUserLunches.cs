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
        /// <returns>A <see cref="DailyLunch"/> object if the userLunch is found, otherwise null</returns>
        Task<UserLunch> GetByIdAsync(Guid userLunchId);

        /// <summary>
        /// Creates a userLunch
        /// </summary>
        /// <param name="userLunch">Lunch to create</param>
        /// <returns>An integer indicating the amount of affected rows</returns>
        Task<UserLunch> CreateAsync(UserLunch userLunch);

        Task<int> CreateUserLunchesAsync(List<UserLunch> userLunches);

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
        Task<List<UserLunch>> GetAsync();

        /// <summary>
        /// Updates a userLunch
        /// </summary>
        /// <param name="userLunch">userLunch to update</param>
        /// <returns>An integer indicating the amount of affected rows</returns>
        Task<UserLunch> UpdateAsync(UserLunch userLunch);

        Task<List<UserLunch>> GetlLunchesByUserByWeekAsync(Guid userId);
    }
}