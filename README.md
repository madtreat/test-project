# TestProject for an interview

Just a little home project for an interview with a company that shall rename unnamed, in case someone else stumbles upon this.

## Instructions
```bash
dotnet watch
```

Then load the default URL in the browser
`https://localhost:7146`

## Notes
While C# provides a `UseDirectoryBrowser` configuration, that kind of defeats the purpose of a significant portion of this project.  Also, this is a challenge for myself, so I would rather make it on my own anyways.

I am totally skipping unit tests, since I want to focus on the mean of the project, and I am able to test the happy paths and some basic sad-paths via VSCode's REST Client extension (see the `test-requests.http` file, and the UI itself)

I would normally build out unit tests for all the known cases: all happy paths, all validation and possible errors thrown, all return values, etc., for each individual API endpoint, for example:
* list files for dir with no children (happy path): returns nothing and 200 success
* list files for dir with children (happy path): returns the files and dirs and 200 success
* list files for dir that does not exist: 404 error
* upload file with same name: 400 error
* upload file to dir that does not exist: 400 error
* upload empty file: 400 error
* upload file (happy path): 200 success or 201 created
* download file that does not exist: 404 not found
* download file in dir that does not exist: 404 not found
* download file that exists (happy path): 200 success
* make directory that already exists: 200 success with warning (idempotent - not a failure in my opinion, for this specific case)
* make directory that does not exist: 200 success or 201 created with filesystem updated
* make directory with parents that do not exist (happy path): 200 success or 201 created, with filesystem updated
* move file that does not exist: 404 not found
* move file to a destination that already exists: 400 bad request
* move file to a destination that does not exist (happy path): 200 success
* copy file that does not exist: 404 not found
* copy file to a destination that already exists: 400 bad request
* copy file to a destination that does not exist (happy path): 200 success
* delete file that does not exist: 404 not found
* delete file that exists: 200 success