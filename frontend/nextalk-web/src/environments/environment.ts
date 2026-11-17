const apiHost = typeof window === 'undefined' ? 'localhost' : window.location.hostname;
const useHttps = typeof window === 'undefined' || window.location.protocol === 'https:';
const apiProtocol = useHttps ? 'https' : 'http';
const apiPort = useHttps ? 7000 : 5129;

export const environment = {
  apiUrl: `${apiProtocol}://${apiHost}:${apiPort}/api`,
  hubUrl: `${apiProtocol}://${apiHost}:${apiPort}/hubs/chat`
};
