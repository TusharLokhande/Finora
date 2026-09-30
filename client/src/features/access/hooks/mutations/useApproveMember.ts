import { approveMember } from "../../api/access.api";
import { useOptimisticMemberMutation } from "./useOptimisticMemberMutation";

export const useApproveMember = () => useOptimisticMemberMutation(approveMember, "Approved", "Couldn't approve this request.");
