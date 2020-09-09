
# docker build . -t emerald && docker run -dt --name emerald emerald

# Only running headless works: https://stackoverflow.com/questions/55844788/how-to-fix-severe-bind-failed-cannot-assign-requested-address-99-while
# docker exec emerald sh -c "echo starting.. && dotnet build && bash run https://rnz.co.nz --headless"

# docker cp emerald:screenshots/ .

# https://medium.com/dot-debug/running-chrome-in-a-docker-container-a55e7f4da4a8
FROM ubuntu:trusty

RUN apt-get update; apt-get clean

# Add a user for running applications.
RUN useradd apps
RUN mkdir -p /home/apps && chown apps:apps /home/apps

# Install x11vnc.
RUN apt-get install -y x11vnc

# Install xvfb.
RUN apt-get install -y xvfb

# Install fluxbox.
RUN apt-get install -y fluxbox

# Install wget.
RUN apt-get install -y wget

# Install wmctrl.
RUN apt-get install -y wmctrl

# Set the Chrome repo.
RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list

# Install Chrome.
RUN apt-get update && apt-get -y install google-chrome-stable

# Emerald

# https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu

RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && sudo dpkg -i packages-microsoft-prod.deb

RUN apt-get install -y apt-transport-https libgdiplus && apt-get update && apt-get install -y dotnet-sdk-3.1

COPY . /

CMD '/chrome-bootstrap.sh'