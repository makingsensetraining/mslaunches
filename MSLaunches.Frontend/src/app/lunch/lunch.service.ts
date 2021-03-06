import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { map, mergeMap } from 'rxjs/operators';
import * as _ from 'lodash';
import * as moment from 'moment';

import { DailyTypedLunches } from '@app/core/Models/daily-typed-lunches.model';
import { UserLunch } from '@app/core/Models/user-lunch.model';
import { UserSelection } from '@app/core/Models/user-selection.model';
import { WeeklyLunches } from '@app/core/Models/weekly-lunches.model';


@Injectable()
export class LunchService {
    private credentialsKey = 'credentials';
    private routes = {
        getLunches: '/lunches',
        getGeneric(userId: string): string {
            return `/users/${userId}/lunches`;
        },
        getAccessor(userId: string, lunchId: string): string {
            return this.getGeneric(userId) + `/${lunchId}`;
        }
    };

    constructor(private httpClient: HttpClient) {
    }

    //#region public methods

    getLunches(startDate?: Date, endDate?: Date): Observable<Array<UserLunch>> {
        return this.httpClient
            .get(this.routes.getLunches)
            .pipe(map(this.mapToArrayOfLaunch.bind(this)));
    }

    getUserLunches(): Observable<Array<UserLunch>> {
        const userId: string =
            JSON.parse(sessionStorage.getItem(this.credentialsKey)).userId;

        return this.getLunches()
            .pipe(
                mergeMap(menu => {
                    return this.httpClient.get(this.routes.getGeneric(userId)).pipe(
                        map((value: any[]) =>
                            this.mergeUserLunchs(this.mapToArrayOfUserSelection(value), menu))
                    );
                })
            );
    }

    mapToWeekly(lunches: Array<UserLunch>): Array<WeeklyLunches> {
        lunches = lunches.sort(this.selectableSorter.bind(this));
        const daily: Array<DailyTypedLunches> = _.map(
            // Groups by date
            _.groupBy(lunches, (result: UserLunch) => result.date),
            // Maps to entity
            (value: UserLunch[], key: string) => ({ date: new Date(key), lunches: value })
        );

        const weekly = _.map(
            // Groups by week
            _.groupBy(daily, (result: DailyTypedLunches) => moment(result.date, 'DD/MM/YYYY').startOf('isoWeek')),
            // maps to entity
            (value: DailyTypedLunches[], key: string) => ({ date: new Date(key), lunches: value })
        );

        return this.fillDates(weekly);
    }

    save(lunch: UserLunch): Observable<string> {
        const userId = JSON.parse(sessionStorage.getItem(this.credentialsKey)).userId;
        if (!lunch.userLunchId || lunch.userLunchId.length === 0) {
            return this.httpClient.post(this.routes.getGeneric(userId), this.mapToBackEnd(lunch))
                .pipe(
                    map((a: any) => a.userLunchId)
                );
        } else {
            return this.httpClient.put(this.routes.getAccessor(userId, lunch.userLunchId), this.mapToBackEnd(lunch))
                .pipe(map((a: any) => lunch.userLunchId));
        }
    }

    delete(lunch: UserLunch): Observable<string> {
        const userId = JSON.parse(sessionStorage.getItem(this.credentialsKey)).userId;
        return this.httpClient.delete(this.routes.getAccessor(userId, lunch.userLunchId))
            .pipe(map(a => 'Deleted'));
    }

    //#endregion

    //#region private methods

    private mergeUserLunchs(userSelections: Array<UserSelection>, menu: Array<UserLunch>): Array<UserLunch> {
        userSelections.forEach(userSelection => {
            // Find the lunch
            const matchingLunch: UserLunch = menu.find(a => a.id === userSelection.lunchId);
            if (matchingLunch) {
                // Find all the daily lunchs
                matchingLunch.isSelected = true;
                menu.filter(a => new Date(a.date).getDate() === new Date(matchingLunch.date).getDate())
                    .forEach(lunch => {
                        lunch.userLunchId = userSelection.id;
                    });
            }
        });

        return menu;
    }

    private mapToBackEnd(lunch: UserLunch): any {
        return {
            LunchId: lunch.id
        };
    }

    private fillDates(weekly: Array<WeeklyLunches>): Array<WeeklyLunches> {
        weekly.forEach(dailyLunch => {
            let startOfTheweek: moment.Moment;
            if (!!dailyLunch.lunches && dailyLunch.lunches.length < 5) {
                startOfTheweek = moment(dailyLunch.date, 'DD/MM/YYYY').startOf('isoWeek');
                for (let i = 0; i < 5; i++) {
                    if (!dailyLunch.lunches.some(item =>
                        item.date.getDate() === startOfTheweek.toDate().getDate())
                    ) {
                        dailyLunch.lunches.push({
                            date: startOfTheweek.toDate(),
                            lunches: new Array<UserLunch>()
                        });
                    }
                    startOfTheweek.add(1, 'days');
                }
                dailyLunch.lunches = dailyLunch.lunches.sort(this.dateSorter);
            }
        });

        return weekly;
    }

    private typeSorter(a: UserLunch, b: UserLunch): number {
        if (a.type < b.type) {
            return -1;
        }
        if (b.type < a.type) {
            return 1;
        }
        return 0;
    }

    private selectableSorter(a: UserLunch, b: UserLunch): number {
        if (a.isSelectable !== b.isSelectable) {
            if (a.isSelectable) {
                return -1;
            }
            if (b.isSelectable) {
                return 1;
            }
        }
        return this.typeSorter(a, b);
    }

    private dateSorter(a: DailyTypedLunches, b: DailyTypedLunches): number {
        if (a.date < b.date) {
            return -1;
        }
        if (b.date < a.date) {
            return 1;
        }
        return 0;
    }

    private mapToArrayOfUserSelection(body: Array<any>): Array<UserSelection> {
        let result: Array<UserSelection> = new Array<UserSelection>();
        result = body.map(this.mapToUserSelection);
        return result;
    }

    private mapToArrayOfLaunch(body: Array<any>): Array<UserLunch> {
        let result: Array<UserLunch> = new Array<UserLunch>();
        result = body.map(this.map);
        return result;
    }

    private mapToUserSelection(body: any): UserSelection {
        let result: UserSelection;
        result = {
            lunchId: body.lunchId,
            id: body.id,
        };

        return result;
    }

    private map(body: any): UserLunch {
        let result: UserLunch;
        result = {
            id: body.id,
            userLunchId: null,
            description: body.meal.name,
            type: body.meal.type.description,
            date: body.date,
            isSelected: false,
            isSelectable: body.meal.type.isSelectable
        };

        return result;
    }

    //#endregion
}
