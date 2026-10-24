import apiClient from '../api/apiClient';
import { API_ENDPOINTS } from '../api/endpoints';
import { mockSuccess } from '../mocks/mockHelper';
import { mockEducatorData } from '../mocks/mockEducator';

export const educatorService = {
  async getClasses() {
    try {
      return await apiClient.get(API_ENDPOINTS.EDUCATOR.CLASSES);
    } catch (err) {
      console.warn('API Educator Classes fallback to mock:', err);
      return mockSuccess(mockEducatorData.classes);
    }
  },

  async claimLicense(licenseKey) {
    return await apiClient.post(API_ENDPOINTS.EDUCATOR.CLAIM_LICENSE, { licenseKey });
  },

  async getClassHeatmap(classId) {
    try {
      return await apiClient.get(API_ENDPOINTS.CLASSES.HEATMAP(classId));
    } catch (err) {
      console.warn('API Class Heatmap fallback to mock:', err);
      return mockSuccess([]);
    }
  },

  async getClassProgress(classId) {
    try {
      return await apiClient.get(API_ENDPOINTS.CLASSES.PROGRESS(classId));
    } catch (err) {
      console.warn('API Class Progress fallback to mock:', err);
      return mockSuccess([]);
    }
  },
};

