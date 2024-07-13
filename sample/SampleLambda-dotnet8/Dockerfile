FROM public.ecr.aws/lambda/dotnet:6 AS fonts-base

FROM public.ecr.aws/lambda/dotnet:8

# Set the image's internal work directory
WORKDIR /var/task
  
# Copy function code
COPY "bin/Release/lambda-publish"  .
COPY --from=fonts-base "/usr/share/fonts" "/usr/share/fonts"
  
# Set the CMD to your handler (could also be done as a parameter override outside of the Dockerfile)
CMD [ "SampleLambda::SampleLambda.HelloWorldHandler::Handle" ]