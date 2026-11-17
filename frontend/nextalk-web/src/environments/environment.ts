const apiHost = typeof window === 'undefined' ? 'localhost' : window.location.hostname;

export const environment = {
  apiUrl: `https://${apiHost}:7000/api`,
  hubUrl: `https://${apiHost}:7000/hubs/chat`
};
